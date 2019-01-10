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
            dataEvent.Url = $"https://1xbetua.com/live/Football/{eventToken["LI"].ToString()}-{dataEvent.Liga.Replace(" ", "-").Replace(".","")}/{dataEvent.IdEvent}-{dataEvent.Team1.Replace(" ", "-")}-{dataEvent.Team2.Replace(" ", "-")}/";

            var goals = eventToken["SC"]["FS"];
            if (goals.Count() != 0)
            {
                dataEvent.SumGoals = Convert.ToInt32(goals["S1"]?.ToString() ?? "0");
                dataEvent.SumGoals += Convert.ToInt32(goals["S2"]?.ToString() ?? "0");
            }

            int milisec = Convert.ToInt32(eventToken["SC"]["TS"]?.ToString() ?? "0");
            dataEvent.MinuteMatch = milisec / 60;

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
                                var elTotB = data.Totals.FirstOrDefault(x => x.Name == name);
                                if (elTotB == null)
                                {
                                    data.Totals.Add(new Total { Name = name, Over = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elTotB.Over = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexTotM:
                                var elTotM = data.Totals.FirstOrDefault(x => x.Name == name);
                                if (elTotM == null)
                                {
                                    data.Totals.Add(new Total { Name = name, Under = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elTotM.Under = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexTot1B:
                                var elTot1B = data.TotalsK1.FirstOrDefault(x => x.Name == name);
                                if (elTot1B == null)
                                {
                                    data.TotalsK1.Add(new Total { Name = name, Over = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elTot1B.Over = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexTot1M:
                                var elTot1M = data.TotalsK1.FirstOrDefault(x => x.Name == name);
                                if (elTot1M == null)
                                {
                                    data.TotalsK1.Add(new Total { Name = name, Under = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elTot1M.Under = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexTot2B:
                                var elTot2B = data.TotalsK2.FirstOrDefault(x => x.Name == name);
                                if (elTot2B == null)
                                {
                                    data.TotalsK2.Add(new Total { Name = name, Over = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elTot2B.Over = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexTot2M:
                                var elTot2M = data.TotalsK2.FirstOrDefault(x => x.Name == name);
                                if (elTot2M == null)
                                {
                                    data.TotalsK2.Add(new Total { Name = name, Under = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elTot2M.Under = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case index3TotB:
                                var el3TotB = data.Total3Events.FirstOrDefault(x => x.Name == name);
                                if (el3TotB == null)
                                {
                                    data.Total3Events.Add(new Total3Event { Name = name, Over = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    el3TotB.Over = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case index3TotEq:
                                var el3TotEq = data.Total3Events.FirstOrDefault(x => x.Name == name);
                                if (el3TotEq == null)
                                {
                                    data.Total3Events.Add(new Total3Event { Name = name, Exactly = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    el3TotEq.Exactly = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case index3TotM:
                                var el3TotM = data.Total3Events.FirstOrDefault(x => x.Name == name);
                                if (el3TotM == null)
                                {
                                    data.Total3Events.Add(new Total3Event { Name = name, Under = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    el3TotM.Under = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexAsTotB:
                                var elAsTotB = data.AsiatTotals.FirstOrDefault(x => x.Name == name);
                                if (elAsTotB == null)
                                {
                                    data.AsiatTotals.Add(new Total { Name = name, Over = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elAsTotB.Over = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexAsTotM:
                                var elAsTotM = data.AsiatTotals.FirstOrDefault(x => x.Name == name);
                                if (elAsTotM == null)
                                {
                                    data.AsiatTotals.Add(new Total { Name = name, Under = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elAsTotM.Under = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexFora1:
                                name = name[0] == '-' ? name : $"+{name}";
                                var oppositeFora1 = GetOppositeForaName(name);
                                var elFora1 = data.Foras.FirstOrDefault(x => x.Name == oppositeFora1);
                                if (elFora1 == null)
                                {
                                    data.Foras.Add(new Fora { Name = name, Team1 = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elFora1.Name = name;
                                    elFora1.Team1 = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexFora2:
                                name = name[0] == '-' ? name : $"+{name}";
                                var oppositeFora2 = GetOppositeForaName(name);
                                var elFora2 = data.Foras.FirstOrDefault(x => x.Name == oppositeFora2);
                                if (elFora2 == null)
                                {
                                    data.Foras.Add(new Fora { Name = name, Team2 = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elFora2.Team2 = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexAsFora1:
                                name = name[0] == '-' ? name : $"+{name}";
                                var oppositeAsFora1 = GetOppositeForaName(name);
                                var elAsFora1 = data.AsiatForas.FirstOrDefault(x => x.Name == oppositeAsFora1);
                                if (elAsFora1 == null)
                                {
                                    data.AsiatForas.Add(new Fora { Name = name, Team1 = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elAsFora1.Name = name;
                                    elAsFora1.Team1 = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case indexAsFora2:
                                name = name[0] == '-' ? name : $"+{name}";
                                var oppositeAsFora2 = GetOppositeForaName(name);
                                var elAsFora2 = data.AsiatForas.FirstOrDefault(x => x.Name == oppositeAsFora2);
                                if (elAsFora2 == null)
                                {
                                    data.AsiatForas.Add(new Fora { Name = name, Team2 = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    elAsFora2.Team2 = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case index3Fora1:
                                var el3Fora1 = data.Handicaps.FirstOrDefault(x => x.Name == name);
                                if (el3Fora1 == null)
                                {
                                    data.Handicaps.Add(new Handicap { Name = name, Team1 = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    el3Fora1.Team1 = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case index3ForaEq:
                                var el3ForaEq = data.Handicaps.FirstOrDefault(x => x.Name == name);
                                if (el3ForaEq == null)
                                {
                                    data.Handicaps.Add(new Handicap { Name = name, Draw = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    el3ForaEq.Draw = ConvertToValueBK(TypeBK.Xbet, val);
                                }
                                break;
                            case index3Fora2:
                                var el3Fora2 = data.Foras.FirstOrDefault(x => x.Name == name);
                                if (el3Fora2 == null)
                                {
                                    data.Handicaps.Add(new Handicap { Name = name, Team2 = ConvertToValueBK(TypeBK.Xbet, val) });
                                }
                                else
                                {
                                    el3Fora2.Team2 = ConvertToValueBK(TypeBK.Xbet, val);
                                }
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

        private string GetOppositeForaName(string name)
        {
            if (name.Contains("-"))
            {
                return name.Replace("-", "+");
            }
            else
            {
                return name.Replace("+", "-");
            }
        }
    }
}