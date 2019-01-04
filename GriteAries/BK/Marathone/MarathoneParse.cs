using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading;
using GriteAries.Models;
using GriteAries.SystemLogging;
using System.Collections.Async;
using System.Linq;

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

        Regex regP1 = new Regex("Match_Result.1.\n>(?<val>.*?)<");
        Regex regP2 = new Regex(@"Match_Result.3.\n>(?<val>.*?)<");
        Regex regX = new Regex(@"Match_Result.draw.\n>(?<val>.*?)<");
        Regex regP1X = new Regex("Result.HD.\n>(?<val>.*?)<");
        Regex regP12 = new Regex(@"Result.HA.\n>(?<val>.*?)<");
        Regex regP2X = new Regex(@"Result.AD.\n>(?<val>.*?)<"); 
        Regex regTotB = new Regex(@"Over (?<val1>.*?).,.mn.:.Total Goals.,.*?epr.:.(?<val2>.*?).,");
        Regex regTotM = new Regex(@"Under (?<val1>.*?).,.mn.:.Total Goals.,.*?epr.:.(?<val2>.*?).,");
        Regex regTot3B = new Regex(@"Over (?<val1>.*?).,.mn.:.Total Goals .3 way..,.*?epr.:.(?<val2>.*?).,");
        Regex regTot3M = new Regex(@"Under (?<val1>.*?).,.mn.:.Total Goals .3 way..,.*?epr.:.(?<val2>.*?).,");
        Regex regTot3E = new Regex(@"Exactly (?<val1>.*?).,.mn.:.Total Goals .3 way..,.*?epr.:.(?<val2>.*?).,");
        Regex regTotAsB = new Regex(@"Over (?<val1>.*?).,.mn.:.Asian Total Goals.,.*?epr.:.(?<val2>.*?).,");
        Regex regTotAsM = new Regex(@"Under (?<val1>.*?).,.mn.:.Asian Total Goals.,.*?epr.:.(?<val2>.*?).,");
      

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


        public async Task SetKoeficient(Data data)
        {
            data.ClearOld();

            Regex regFora1 = new Regex($"{data.Team1}..(?<val1>.*?)..,.mn.:.To Win Match With Handicap.,.*?epr.:.(?<val2>.*?).,");
            Regex regFora2 = new Regex($"{data.Team2}..(?<val1>.*?)..,.mn.:.To Win Match With Handicap.,.*?epr.:.(?<val2>.*?).,");
            Regex regHand1 = new Regex($"{data.Team1}..(?<val1>.*?)..,.mn.:.To Win Match With Handicap .3 way..,.*?epr.:.(?<val2>.*?).,");
            Regex regHand2 = new Regex($"{data.Team2}..(?<val1>.*?)..,.mn.:.To Win Match With Handicap .3 way..,.*?epr.:.(?<val2>.*?).,");
            Regex regHandD = new Regex(@"Draw..(?<val1>.*?)..,.mn.:.To Win Match With Handicap .3 way..,.*?epr.:.(?<val2>.*?).,");
            Regex regForaAs1 = new Regex($"{data.Team1}..(?<val1>.*?)..,.mn.:.To Win Match With Asian Handicap.,.*?epr.:.(?<val2>.*?).,");
            Regex regForaAs2 = new Regex($"{data.Team2}..(?<val1>.*?)..,.mn.:.To Win Match With Asian Handicap.,.*?epr.:.(?<val2>.*?).,");
            Regex regTotT1B = new Regex($"Over (?<val1>.*?).,.mn.:.Total Goals .{data.Team1}..,.*?epr.:.(?<val2>.*?).,");
            Regex regTotT1M = new Regex($"Under (?<val1>.*?).,.mn.:.Total Goals .{data.Team1}..,.*?epr.:.(?<val2>.*?).,");
            Regex regTotT2B = new Regex($"Over (?<val1>.*?).,.mn.:.Total Goals .{data.Team2}..,.*?epr.:.(?<val2>.*?).,");
            Regex regTotT2M = new Regex($"Under (?<val1>.*?).,.mn.:.Total Goals .{data.Team2}..,.*?epr.:.(?<val2>.*?).,");

            try
            {
                var kod = await _web.GetPageEvent(data.IdEvent);
                var str = kod.Text;

                data.P1 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP1.Match(str).Groups["val"].Value) };
                data.P2 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP2.Match(str).Groups["val"].Value) };
                data.X = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regX.Match(str).Groups["val"].Value) };
                data.X1 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP1X.Match(str).Groups["val"].Value) };
                data.P12 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP12.Match(str).Groups["val"].Value) };
                data.X2 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP2X.Match(str).Groups["val"].Value) };

                if (str.Contains("\"To Win Match With Handicap\""))
                {
                    data.Foras.AddRange(GetFora(str, regFora1, regFora2));
                }

                if (str.Contains("\"To Win Match With Handicap (3 way)\""))
                {
                    data.Handicaps.AddRange(GetHandicap(str, regHand1, regHand2, regHandD));
                }

                if (str.Contains("\"To Win Match With Asian Handicap\""))
                {
                    data.AsiatForas.AddRange(GetFora(str, regForaAs1, regForaAs2));
                }

                if (str.Contains("\"Total Goals\""))
                {
                    data.Totals.AddRange(GetTotal(str, regTotB, regTotM));
                }

                if (str.Contains("\"Total Goals (3 way)\""))
                {
                    data.Total3Events.AddRange(GetTot3Event(str, regTot3B, regTot3M, regTot3E));
                }

                if (str.Contains("\"Asian Total Goals\""))
                {
                    data.AsiatTotals.AddRange(GetTotal(str, regTotAsB, regTotAsM));
                }

                if (str.Contains("\"Total Goals (" + data.Team1 + ")\""))
                {
                    data.TotalsK1.AddRange(GetTotal(str, regTotT1B, regTotT1M));
                }

                if (str.Contains("\"Total Goals (" + data.Team2 + ")\""))
                {
                    data.TotalsK2.AddRange(GetTotal(str, regTotT2B, regTotT2M));
                }
            }
            catch (Exception e)
            {
                string log = $"Error in parse koef\n{e.ToString()}";
                await _logging.WriteLog(log);
            }
        }

        #region Methods from Fora
        public List<Fora> GetFora(string str, Regex reg1, Regex reg2)
        {
            List<Fora> list = new List<Fora>();

            var collFora1 = reg1.Matches(str);
            var collFora2 = reg2.Matches(str);

            for (int i = 0; i < collFora1.Count; i++)
            {
                Fora fora = new Fora();

                string nameFora = collFora1[i].Groups["val1"].Value;

                if (nameFora.Contains(","))
                {
                    string[] name = collFora1[i].Groups["val1"].Value.Split(',');

                    if (name[0].Contains("-") || name[1].Contains("-"))
                    {
                        fora.Name = Convert.ToString((ConvertToFloat(name[0]) + ConvertToFloat(name[1])) / 2);
                    }
                    else
                    {
                        fora.Name = "+" + Convert.ToString((ConvertToFloat(name[0]) + ConvertToFloat(name[1])) / 2);
                    }
                }
                else
                {
                    fora.Name = nameFora;
                }

                var fora1 = collFora1[i].Groups["val2"].Value;
                fora.Team1 = ConvertToValueBK(TypeBK.Marathone, fora1);

                var fora2 = 
                fora.Team2 = GetValueFora(collFora2, collFora1[i].Groups["val1"].Value);

                list.Add(fora);
            }

            return list;
        }

        public List<Handicap> GetHandicap(string str, Regex reg1, Regex reg2, Regex reg3)
        {
            List<Handicap> list = new List<Handicap>();

            var collHand1 = reg1.Matches(str);
            var collHand2 = reg2.Matches(str);
            var collHandD = reg3.Matches(str);

            for (int i = 0; i < collHand1.Count; i++)
            {
                Handicap handicap = new Handicap();

                handicap.Name = collHand1[i].Groups["val1"].Value;

                var team1 = collHand1[i].Groups["val2"].Value;
                handicap.Team1 = ConvertToValueBK(TypeBK.Marathone, team1);

                handicap.Team2 = GetValueFora(collHand2, handicap.Name);

                var draw = collHandD.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(handicap.Name)).Groups["val2"].Value;
                handicap.Draw = ConvertToValueBK(TypeBK.Marathone, draw);

                list.Add(handicap);
            }

            return list;
        }

        public ValueBK GetValueFora(MatchCollection match, string name1)
        {
            ValueBK valueBK = new ValueBK(TypeBK.Marathone);

            if (match.Count == 0)
            {
                valueBK.Value = 0;
            }
            else
            {
                string temp;
                if (name1.Contains("-"))
                {
                    temp = name1.Replace("-", "+");
                }
                else
                {
                    temp = name1.Replace("+", "-");
                }

                valueBK.Value = ConvertToFloat(match.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(temp)).Groups["val2"].Value);
            }

            return valueBK;
        }

        #endregion

        #region Methods from Total
        public List<Total> GetTotal(string str, Regex reg1, Regex reg2)
        {
            List<Total> list = new List<Total>();
            var collTotB = reg1.Matches(str);
            var collTotM = reg2.Matches(str);

            for (int i = 0; i < collTotB.Count; i++)
            {
                Total total = new Total();
                string temp = collTotB[i].Groups["val1"].Value;

                if (temp.Contains(","))
                {
                    string[] name = temp.Split(',');
                    total.Name = Convert.ToString((ConvertToFloat(name[0]) + ConvertToFloat(name[1])) / 2);
                }
                else
                {
                    total.Name = temp;
                }

                var valOver = collTotB[i].Groups["val2"].Value;
                total.Over = ConvertToValueBK(TypeBK.Marathone, valOver);

                var valUnder = collTotM.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(temp)).Groups["val2"].Value;
                total.Under = ConvertToValueBK(TypeBK.Marathone, valUnder);

                list.Add(total);
            }

            return list;
        }

        public List<Total3Event> GetTot3Event(string str, Regex reg1, Regex reg2, Regex reg3)
        {
            List<Total3Event> list = new List<Total3Event>();
            var collTotB = reg1.Matches(str);
            var collTotM = reg2.Matches(str);
            var collTotE = reg3.Matches(str);

            for (int i = 0; i < collTotB.Count; i++)
            {
                Total3Event total = new Total3Event();

                total.Name = collTotB[i].Groups["val1"].Value;

                var valOver = collTotB[i].Groups["val2"].Value;
                total.Over = ConvertToValueBK(TypeBK.Marathone, valOver);

                var valUnder = collTotM.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(total.Name)).Groups["val2"].Value;
                total.Under = ConvertToValueBK(TypeBK.Marathone, valUnder);

                var valExact = collTotE.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(total.Name)).Groups["val2"].Value;
                total.Exactly = ConvertToValueBK(TypeBK.Marathone, valExact);

                list.Add(total);
            }
            return list;
        }
        #endregion

    }

}