using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GriteAries.SystemLogging;
using GriteAries.Models;
using System.Text.RegularExpressions;

namespace GriteAries.BK.XBet
{
    public class XBetParse : Bukmeker, IBukmekerParse
    {
        private XBetWeb _web;
        private Logging _logging;
        private int CountParalelThread;

        public XBetParse()
        {
            string agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";

            CountParalelThread = 10;
            _logging = new Logging();
            _web = new XBetWeb(_logging, agent);
        }

        public async Task<List<Data>> GetStartData(TypeSport typeSport)
        {
            var datas = new List<Data>();

            var kod = await _web.GetPageLiveSport(typeSport.ToString());
            if (kod == null)
            {
                return null;
            }

            var kodSport = await CutOut(kod, "{\"live\"", "}]}}");
            if (kodSport == null)
            {
                return null;
            }

            var jsonSport = JObject.Parse(kodSport);
            var arrEvent = (JArray)jsonSport["live"]["Value"];

            var usedEvent = Container.GetUsedId(TypeBK.Xbet);

            foreach (var item in arrEvent)
            {
                var data = SetDataEvent(item, usedEvent);

                if (data != null)
                {
                    datas.Add(data);
                }
            }

            return datas;
        }

        private async Task<string> CutOut(string kod, string pattern1, string pattern2)
        {
            int indexStart = kod.IndexOf(pattern1);
            if (indexStart < 0)
            {
                await _logging.WriteLog("XBET\tERRORS no Live Matches");
                return null;
            }

            int indexEnd = kod.IndexOf(pattern2, indexStart);
            if (indexEnd < 0)
            {
                await _logging.WriteLog("XBET\tERRORS in CutOut");
                return null;
            }

            var rez = kod.Substring(indexStart, indexEnd - indexStart + pattern2.Length);

            return rez;
        }

        private Data SetDataEvent(JToken eventToken, List<int> usedId)
        {
            int id = Convert.ToInt32(eventToken["I"].ToString());
            if (usedId.Contains(id))
            {
                return null;
            }

            Data dataEvent = new Data();

            dataEvent.Bukmeker = TypeBK.Xbet;
            dataEvent.IdEvent = id;
            dataEvent.Liga = eventToken["L"].ToString();
            dataEvent.Team1 = eventToken["O1"].ToString();
            dataEvent.Team2 = eventToken["O2"].ToString();
            dataEvent.Url = $"https://1xbetua.com/live/Football/{eventToken["LI"].ToString()}-{dataEvent.Liga.Replace(".", "").Replace(" ", "-")}/{dataEvent.IdEvent}-{dataEvent.Team1.Replace("(", "").Replace(")", "").Replace(" ", "-")}-{dataEvent.Team2.Replace("(", "").Replace(")", "").Replace(" ", "-")}/";

            var goals = eventToken["SC"]["FS"];
            if (goals.Count() != 0)
            {
                dataEvent.SumGoals = Convert.ToInt32(goals["S1"]?.ToString() ?? "0");
                dataEvent.SumGoals += Convert.ToInt32(goals["S2"]?.ToString() ?? "0");
            }

            int milisec = Convert.ToInt32(eventToken["SC"]["TS"]?.ToString() ?? "0");
            dataEvent.MinuteMatch = milisec / 60;

            if (dataEvent.MinuteMatch > MaxMinuteMatchFootball)
            {
                return null;
            }

            return dataEvent;
        }


        public async Task SetKoeficient(Data data)
        {
            const int indexPX = 1, index2PX = 8, indexTot = 17, indexFora = 2, indexTotT1 = 15, indexTotT2 = 62, indexAsTot = 99, indexAsFora = 2854, index3EvTot = 87, index3EvFora = 27;

            try
            {
                var kod = await _web.GetInfoEvent(data.IdEvent);
                var json = JObject.Parse(kod);
                var arrEvents = (JArray)json["Value"]["GE"];
                data.ClearOld();

                int milisec = Convert.ToInt32(json["Value"]["SC"]["TS"]?.ToString() ?? "0");
                data.MinuteMatch = milisec / 60;

                foreach (var even in arrEvents)
                {
                    var arrDatas = (JArray)even["E"];
                    var typeData = (int)even["G"];

                    if (typeData == indexPX || typeData == index2PX)
                    {
                        SetPX(ref data, arrDatas);
                    }
                    else if (typeData == indexTot || typeData == indexFora || typeData == indexTotT1 || typeData == indexTotT2 || typeData == indexAsTot || typeData == indexAsFora || typeData == index3EvTot || typeData == index3EvFora)
                    {
                        SetTotAndFora(ref data, arrDatas);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private void SetPX(ref Data data, JArray arrData)
        {
            const int indexData = 0, indexP1 = 1, indexX = 2, indexP2 = 3, indexP1X = 4, indexP12 = 5, indexP2X = 6;

            try
            {
                foreach (var jData in arrData)
                {
                    var type = (int)jData[indexData]["T"];
                    var value = jData[indexData]["C"].ToString();
                    var stateNull = jData[indexData]["B"]?.ToString() ?? "False";

                    if (stateNull.Equals("True"))
                    {
                        continue;
                    }

                    switch (type)
                    {
                        case indexP1:
                            data.P1 = ConvertToValueBK(TypeBK.Xbet, value);
                            break;
                        case indexX:
                            data.X = ConvertToValueBK(TypeBK.Xbet, value);
                            break;
                        case indexP2:
                            data.P2 = ConvertToValueBK(TypeBK.Xbet, value);
                            break;
                        case indexP1X:
                            data.X1 = ConvertToValueBK(TypeBK.Xbet, value);
                            break;
                        case indexP12:
                            data.P12 = ConvertToValueBK(TypeBK.Xbet, value);
                            break;
                        case indexP2X:
                            data.X2 = ConvertToValueBK(TypeBK.Xbet, value);
                            break;
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private void SetTotAndFora(ref Data data, JArray arrData)
        {
            const int indexTotB = 9, indexTotM = 10, indexFora1 = 7, indexFora2 = 8, indexTot1B = 11, indexTot1M = 12, indexTot2B = 13, indexTot2M = 14, index3TotB = 739,
                index3TotEq = 741, index3TotM = 743, index3Fora1 = 424, index3ForaEq = 425, index3Fora2 = 426, indexAsTotB = 3827, indexAsTotM = 3828, indexAsFora1 = 3829, indexAsFora2 = 3830;
            try
            {
                foreach (JArray jDatas in arrData)
                {
                    foreach (var item in jDatas)
                    {
                        var type = (int)item["T"];
                        var name = item["P"]?.ToString() ?? "0";
                        var val = item["C"].ToString();
                        var stateNull = item["B"]?.ToString() ?? "False";

                        if (stateNull.Equals("True"))
                        {
                            continue;
                        }

                        switch (type)
                        {
                            case indexTotB:
                                SetTotal(data.Totals, name, val, true);
                                break;
                            case indexTotM:
                                SetTotal(data.Totals, name, val, false);
                                break;
                            case indexTot1B:
                                SetTotal(data.TotalsK1, name, val, true);
                                break;
                            case indexTot1M:
                                SetTotal(data.TotalsK1, name, val, false);
                                break;
                            case indexTot2B:
                                SetTotal(data.TotalsK2, name, val, true);
                                break;
                            case indexTot2M:
                                SetTotal(data.TotalsK2, name, val, false);
                                break;
                            case index3TotB:
                                SetTotal(data.Total3Events, name, val, "Over");
                                break;
                            case index3TotEq:
                                SetTotal(data.Total3Events, name, val, "Exactly");
                                break;
                            case index3TotM:
                                SetTotal(data.Total3Events, name, val, "Under");
                                break;
                            case indexAsTotB:
                                SetTotal(data.AsiatTotals, name, val, true);
                                break;
                            case indexAsTotM:
                                SetTotal(data.AsiatTotals, name, val, false);
                                break;
                            case indexFora1:
                                SetFora(data.Foras, name, val, true);
                                break;
                            case indexFora2:
                                SetFora(data.Foras, name, val, false);
                                break;
                            case indexAsFora1:
                                SetFora(data.AsiatForas, name, val, true);
                                break;
                            case indexAsFora2:
                                SetFora(data.AsiatForas, name, val, false);
                                break;
                            case index3Fora1:
                                SetFora(data.Handicaps, name, val, "Team1");
                                break;
                            case index3ForaEq:
                                SetFora(data.Handicaps, name, val, "Draw");
                                break;
                            case index3Fora2:
                                SetFora(data.Handicaps, name, val, "Team2");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        private void SetTotal(List<Total> totals, string name, string value, bool stateOver)
        {
            var tot = totals.FirstOrDefault(x => x.Name == name);

            if (tot == null)
            {
                if (stateOver)
                {
                    totals.Add(new Total { Name = name, Over = ConvertToValueBK(TypeBK.Xbet, value) });
                }
                else
                {
                    totals.Add(new Total { Name = name, Under = ConvertToValueBK(TypeBK.Xbet, value) });
                }
            }
            else
            {
                if (stateOver)
                {
                    tot.Over = ConvertToValueBK(TypeBK.Xbet, value);
                }
                else
                {
                    tot.Under = ConvertToValueBK(TypeBK.Xbet, value);
                }
            }
        }

        private void SetTotal(List<Total3Event> totals, string name, string value, string typeEv)
        {
            var tot = totals.FirstOrDefault(x => x.Name == name);

            if (tot == null)
            {
                if (typeEv.Contains("Over"))
                {
                    totals.Add(new Total3Event { Name = name, Over = ConvertToValueBK(TypeBK.Xbet, value) });
                }
                else if (name.Contains("Under"))
                {
                    totals.Add(new Total3Event { Name = name, Under = ConvertToValueBK(TypeBK.Xbet, value) });
                }
                else
                {
                    totals.Add(new Total3Event { Name = name, Exactly = ConvertToValueBK(TypeBK.Xbet, value) });
                }
            }
            else
            {
                if (name.Contains("Over"))
                {
                    tot.Over = ConvertToValueBK(TypeBK.Xbet, value);
                }
                else if (name.Contains("Under"))
                {
                    tot.Under = ConvertToValueBK(TypeBK.Xbet, value);
                }
                else
                {
                    tot.Exactly = ConvertToValueBK(TypeBK.Xbet, value);
                }
            }
        }

        private void SetFora(List<Fora> foras, string name, string value, bool stateTeam1)
        {
            if (!name.Equals("0"))
            {
                name = name[0] == '-' ? name : $"+{name}";
            }

            if (!stateTeam1)
            {
                name = GetOppositeForaName(name);
            }

            var fora = foras.FirstOrDefault(x => x.Name == name);

            if (fora == null)
            {
                if (stateTeam1)
                {
                    foras.Add(new Fora { Name = name, Team1 = ConvertToValueBK(TypeBK.Xbet, value) });
                }
                else
                {
                    foras.Add(new Fora { Name = name, Team2 = ConvertToValueBK(TypeBK.Xbet, value) });
                }
            }
            else
            {
                if (stateTeam1)
                {
                    fora.Team1 = ConvertToValueBK(TypeBK.Xbet, value);
                }
                else
                {
                    fora.Team2 = ConvertToValueBK(TypeBK.Xbet, value);
                }
            }
        }

        private void SetFora(List<Handicap> foras, string name, string value, string typeEv)
        {
            if (!name.Equals("0"))
            {
                name = name[0] == '-' ? name : $"+{name}";
            }

            var fora = foras.FirstOrDefault(x => x.Name == name);

            if (fora == null)
            {
                if (typeEv.Equals("Team1"))
                {
                    foras.Add(new Handicap { Name = name, Team1 = ConvertToValueBK(TypeBK.Xbet, value) });
                }
                else if (typeEv.Equals("Team2"))
                {
                    foras.Add(new Handicap { Name = name, Team2 = ConvertToValueBK(TypeBK.Xbet, value) });
                }
                else
                {
                    foras.Add(new Handicap { Name = name, Draw = ConvertToValueBK(TypeBK.Xbet, value) });
                }
            }
            else
            {
                if (typeEv.Equals("Team1"))
                {
                    fora.Team1 = ConvertToValueBK(TypeBK.Xbet, value);
                }
                else if (typeEv.Equals("Team2"))
                {
                    fora.Team2 = ConvertToValueBK(TypeBK.Xbet, value);
                }
                else
                {
                    fora.Draw = ConvertToValueBK(TypeBK.Xbet, value);
                }
            }
        }
    }
}