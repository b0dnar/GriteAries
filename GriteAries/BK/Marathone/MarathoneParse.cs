using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using GriteAries.Models;
using GriteAries.SystemLogging;
using System.Collections.Async;

namespace GriteAries.BK.Marathone
{
    public class MarathoneParse : IBukmekerParse
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

            var idEvens = await GetIdEvent(typeSport, kod);
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

        private async Task<List<int>> GetIdEvent(TypeSport typeSport, string kodPage)
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
                    idEvens.Add(Convert.ToInt32(value));
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
                dataEvent.Url = $"https://www.marathonbet.com/en/live/{idEvent}";
                var nameMatch = regNameTeams.Match(html.Text).Groups["val"].Value;
                var nameTeams = Regex.Split(nameMatch, " vs ");

                dataEvent.Team1 = nameTeams[0];
                dataEvent.Team2 = nameTeams[1];

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
    }

}