using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using GriteAries.Models;
using GriteAries.SystemLogging;
using System.Collections.Async;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace GriteAries.BK.Marathone
{
    public class MarathoneParse : Bukmeker, IBukmekerParse
    {
        private MarathoneWeb _web;
        private Logging _logging;
        private int CountParalelThread;

        Regex regEvent = new Regex(@"href.:..en.live.(?<val>.*?).,");
        Regex regNum = new Regex("^[0-9]+$");
        Regex regNameTeams = new Regex("data-event-name=.(?<val>.*?). data-live");
        Regex regGoals = new Regex(@" (?<val1>\d*?):(?<val2>\d*?) ");
        Regex regTime = new Regex(@" (?<val>\d*?):");

        public MarathoneParse()
        {
            string agent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/71.0.3578.98 Safari/537.36";

            CountParalelThread = 10;
            _logging = new Logging();
            _web = new MarathoneWeb(_logging, agent);
        }

        public async Task<List<Data>> GetStartData(TypeSport typeSport)
        {
            var datas = new List<Data>();

            var kod = await _web.GetPageLiveSport();
            if (kod == null)
            {
                return null;
            }

            var usedIdEvent = Container.GetUsedId(TypeBK.Marathone);

            var idEvens = await GetIdEvent(typeSport, kod, usedIdEvent);
            if (idEvens == null)
            {
                return null;
            }

            await idEvens.ParallelForEachAsync(async item =>
            {
                var data = await SetDataEvent(item);

                if (data != null)
                {
                    datas.Add(data);
                }
            }, maxDegreeOfParalellism: CountParalelThread);

            return datas;
        }

        private async Task<List<int>> GetIdEvent(TypeSport typeSport, string kodPage, List<int> usedId)
        {
            List<int> idEvens = new List<int>();
            string kodSport;

            switch (typeSport)
            {
                case TypeSport.Football:
                    string[] arrFoot = { "sport\",\"label\":\"Football", "sport\",\"label\":", "</script>" };
                    kodSport = CutOutText(kodPage, arrFoot[0], arrFoot[1], arrFoot[2]);
                    break;
                default:
                    await _logging.WriteLog("Не правильно вказано тип спорту!");
                    return null;
            }

            var collectionEvents = regEvent.Matches(kodSport);
            if (collectionEvents.Count == 0)
            {
                string log = String.Format("Не має лайв подій в спотрі {0}", typeSport.ToString());
                await _logging.WriteLog(log);

                return null;
            }

            foreach (Match id in collectionEvents)
            {
                string value = id.Groups["val"].Value;

                if (regNum.IsMatch(value))
                {
                    int idEv = Convert.ToInt32(value);

                    if (!usedId.Contains(idEv))
                    {
                        idEvens.Add(idEv);
                    }
                }
            }

            //remove id typeSport
            if (idEvens.Count > 0)
            {
                idEvens.RemoveAt(0);
            }

            return idEvens;
        }

        private string CutOutText(string str, string pattetn1, string pattern2, string pattern3)
        {
            int indexStart = str.IndexOf(pattetn1);

            if (indexStart < 0)
                return "";

            int indexEnd = str.IndexOf(pattern2, indexStart + 10);

            if (indexEnd < 0)
                indexEnd = str.IndexOf(pattern3, indexStart + 10);

            return str.Substring(indexStart, indexEnd - indexStart);
        }

        private async Task<Data> SetDataEvent(int idEvent)
        {
            Data dataEvent = new Data();

            var html = await _web.GetPageEvent(idEvent);

            string[] arrGoalsTime;
            try
            {
                dataEvent.Bukmeker = TypeBK.Marathone;
                dataEvent.IdEvent = idEvent;
                dataEvent.Url = $"https://www.marathonbet.com/en/live/{idEvent}";
                var nameMatch = regNameTeams.Match(html.Text).Groups["val"].Value;
                var nameTeams = Regex.Split(nameMatch, " vs ");

                dataEvent.Team1 = nameTeams[0].Replace("&amp;", "&");
                dataEvent.Team2 = nameTeams[1].Replace("&amp;", "&");

                dataEvent.Liga = html.DocumentNode.SelectSingleNode("//h2[@class='category-label']").InnerText;

                var dataGoalsAndTime = html.DocumentNode.SelectSingleNode("//div[@class='cl-left red']").InnerText;
                arrGoalsTime = Regex.Split(dataGoalsAndTime, "\n \n");

                int goal1 = Convert.ToInt32(regGoals.Match(arrGoalsTime[0]).Groups["val1"].Value);
                int goal2 = Convert.ToInt32(regGoals.Match(arrGoalsTime[0]).Groups["val2"].Value);
                dataEvent.SumGoals = goal1 + goal2;

                if (arrGoalsTime[1].Contains("HT"))
                {
                    dataEvent.MinuteMatch = 45;
                }
                else
                {
                    if (arrGoalsTime[1].Contains("90"))
                    {
                        dataEvent.MinuteMatch = 90;
                    }
                    else
                    {
                        int time = Convert.ToInt32(regTime.Match(arrGoalsTime[1]).Groups["val"].Value);
                        dataEvent.MinuteMatch = time;
                    }
                }
            }
            catch (Exception e)
            {
                string log = String.Format("ERROR in SetDataEvent\n{0}", e.ToString());
                await _logging.WriteLog(log);

                return null;
            }

            return dataEvent;
        }


        public async Task SetKoeficient(Data data)
        {
            string totalTeam1 = $"Total Goals ({data.Team1})";
            try
            {
                var html = await _web.GetPageEvent(data.IdEvent);
                var el = html.DocumentNode.SelectNodes("//td");

                data.ClearOld();

                foreach (var item in el)
                {
                    var str = item.Attributes["data-sel"]?.Value ?? "";
                    if (str.Equals(""))
                    {
                        continue;
                    }

                    var json = JObject.Parse(str);
                    var typeEvent = json["mn"].ToString();

                    switch (typeEvent)
                    {
                        case "Match Result":
                            SetPX(data, json);
                            break;
                        case "Result":
                            Set2PX(data, json);
                            break;
                        case "Total Goals":
                            SetTotal(data.Totals, json);
                            break;
                        case "Total Goals (3 way)":
                            SetTotal(data.Total3Events, json);
                            break;
                        case "Asian Total Goals":
                            SetTotal(data.AsiatTotals, json);
                            break;
                        case "To Win Match With Handicap":
                            SetFora(data.Foras, json, data.Team1, data.Team2);
                            break;
                        case "To Win Match With Handicap (3 way)":
                            SetFora(data.Handicaps, json, data.Team1, data.Team2);
                            break;
                        case "To Win Match With Asian Handicap":
                            SetFora(data.AsiatForas, json, data.Team1, data.Team2);
                            break;
                        default:
                            if (typeEvent.Equals($"Total Goals ({data.Team1})"))
                            {
                                SetTotal(data.TotalsK1, json);
                            }
                            else if (typeEvent.Equals($"Total Goals ({data.Team2})"))
                            {
                                SetTotal(data.TotalsK2, json);
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                string log = $"Error in parse koef\n{e.ToString()}";
                await _logging.WriteLog(log);
            }
        }

        private void SetPX(Data data, JObject json)
        {
            var name = json["sn"].ToString();
            var value = json["epr"].ToString();

            if (name.Equals($"{data.Team1} To Win"))
            {
                data.P1 = ConvertToValueBK(TypeBK.Marathone, value);
            }
            else if (name.Equals($"{data.Team2} To Win"))
            {
                data.P2 = ConvertToValueBK(TypeBK.Marathone, value);
            }
            else
            {
                data.X = ConvertToValueBK(TypeBK.Marathone, value);
            }
        }

        private void Set2PX(Data data, JObject json)
        {
            var name = json["sn"].ToString();
            var value = json["epr"].ToString();

            if (name.Equals($"{data.Team1} To Win or Draw"))
            {
                data.X1 = ConvertToValueBK(TypeBK.Marathone, value);
            }
            else if (name.Equals($"{data.Team2} To Win or Draw"))
            {
                data.X2 = ConvertToValueBK(TypeBK.Marathone, value);
            }
            else
            {
                data.P12 = ConvertToValueBK(TypeBK.Marathone, value);
            }
        }

        private void SetTotal(List<Total> totals, JObject json)
        {
            var name = json["sn"].ToString();
            var value = json["epr"].ToString();
            string nameTot = name.Replace("Over ", "").Replace("Under ", "");

            if(name.Equals("Odd") || name.Equals("Even"))
            {
                return;
            }

            if (nameTot.Contains(","))
            {
                var temp = nameTot.Split(',');
                nameTot = Convert.ToString((ConvertToFloat(temp[0]) + ConvertToFloat(temp[1])) / 2);
            }

            var tot = totals.FirstOrDefault(x => x.Name == nameTot);
            if (tot == null)
            {
                if (name.Contains("Over"))
                {
                    totals.Add(new Total { Name = nameTot, Over = ConvertToValueBK(TypeBK.Marathone, value) });
                }
                else
                {
                    totals.Add(new Total { Name = nameTot, Under = ConvertToValueBK(TypeBK.Marathone, value) });
                }
            }
            else
            {
                if (name.Contains("Over"))
                {
                    tot.Over = ConvertToValueBK(TypeBK.Marathone, value);
                }
                else
                {
                    tot.Under = ConvertToValueBK(TypeBK.Marathone, value);
                }
            }
        }

        private void SetTotal(List<Total3Event> totals, JObject json)
        {
            var name = json["sn"].ToString();
            var value = json["epr"].ToString();
            string nameTot = name.Replace("Over ", "").Replace("Under ", "").Replace("Exactly ", "");

            var tot = totals.FirstOrDefault(x => x.Name == nameTot);
            if (tot == null)
            {
                if (name.Contains("Over"))
                {
                    totals.Add(new Total3Event { Name = nameTot, Over = ConvertToValueBK(TypeBK.Marathone, value) });
                }
                else if (name.Contains("Under"))
                {
                    totals.Add(new Total3Event { Name = nameTot, Under = ConvertToValueBK(TypeBK.Marathone, value) });
                }
                else
                {
                    totals.Add(new Total3Event { Name = nameTot, Exactly = ConvertToValueBK(TypeBK.Marathone, value) });
                }
            }
            else
            {
                if (name.Contains("Over"))
                {
                    tot.Over = ConvertToValueBK(TypeBK.Marathone, value);
                }
                else if (name.Contains("Under"))
                {
                    tot.Under = ConvertToValueBK(TypeBK.Marathone, value);
                }
                else
                {
                    tot.Exactly = ConvertToValueBK(TypeBK.Marathone, value);
                }
            }
        }

        private void SetFora(List<Fora> foras, JObject json, string name1, string name2)
        {
            var name = json["sn"].ToString();
            var value = json["epr"].ToString();
            string nameFora = name.Replace($"{name1} (", "").Replace($"{name2} (", "").Replace(")", "");

            if(nameFora.Contains(","))
            {
                var temp = nameFora.Split(',');
                nameFora = Convert.ToString((ConvertToFloat(temp[0]) + ConvertToFloat(temp[1])) / 2);
                if(temp[0].Contains("+") || temp[1].Contains("+"))
                {
                    nameFora = $"+{nameFora}";
                }
            }

            if (name.Contains(name2))
            {
                nameFora = GetOppositeForaName(nameFora);
            }

            Fora fora = foras.FirstOrDefault(x => x.Name == nameFora);

            if (fora == null)
            {
                if (name.Contains(name1))
                {
                    foras.Add(new Fora { Name = nameFora, Team1 = ConvertToValueBK(TypeBK.Marathone, value) });
                }
                else 
                {
                    foras.Add(new Fora { Name = nameFora, Team2 = ConvertToValueBK(TypeBK.Marathone, value) });
                }
            }
            else
            {
                if (name.Contains(name1))
                {
                    fora.Team1 = ConvertToValueBK(TypeBK.Marathone, value);
                }
                else
                {
                    fora.Team2 = ConvertToValueBK(TypeBK.Marathone, value);
                }
            }
        }

        private void SetFora(List<Handicap> foras, JObject json, string name1, string name2)
        {
            var name = json["sn"].ToString();
            var value = json["epr"].ToString();
            string nameFora = name.Replace($"{name1} (", "").Replace($"{name2} (", "").Replace("Draw (", "").Replace(")", "");

            if (name.Contains(name2))
            {
                nameFora = GetOppositeForaName(nameFora);
            }

            Handicap fora = foras.FirstOrDefault(x => x.Name == nameFora);
            
            if (fora == null)
            {
                if (name.Contains(name1))
                {
                    foras.Add(new Handicap { Name = nameFora, Team1 = ConvertToValueBK(TypeBK.Marathone, value) });
                }
                else if (name.Contains(name2))
                {
                    foras.Add(new Handicap { Name = nameFora, Team2 = ConvertToValueBK(TypeBK.Marathone, value) });
                }
                else
                {
                    foras.Add(new Handicap { Name = nameFora, Draw = ConvertToValueBK(TypeBK.Marathone, value) });
                }
            }
            else
            {
                if (name.Contains(name1))
                {
                    fora.Team1 = ConvertToValueBK(TypeBK.Marathone, value);
                }
                else if (name.Contains(name2))
                {
                    fora.Team2 = ConvertToValueBK(TypeBK.Marathone, value);
                }
                else
                {
                    fora.Draw = ConvertToValueBK(TypeBK.Marathone, value);
                }
            }
        }
    }

}