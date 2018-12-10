using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GriteAries.Request;
using GriteAries.Models;
using HtmlAgilityPack;

namespace GriteAries.BK.Parse
{
    public class Marathon : OutputData, Bukmeker
    {
        public void ParseLiveFootball()
        {
            string urlLive = BaseUrlBK.GetBaseUrl(BaseUrl.MarathonLive);
            string kodPage = Base.HttpGet(urlLive);
            kodPage = CutOutText(kodPage, "sport\",\"label\":\"Football", "sport\",\"label\":", "</script>");

            if (kodPage.Equals(""))
                return;

            List<string> idEvent = GetIdEvent(kodPage);

            foreach (var item in idEvent)
            {
                string urlEvent = BaseUrlBK.GetBaseUrl(BaseUrl.MarathonLive) + item;
                string strEvent = Base.HttpGet(urlEvent);

                DataFootball football = GetInfoFootball(strEvent);
                football.Bukmeker = "Marathon";
                football.Url = urlEvent;
                SetKoefFootball(ref football, strEvent);

                dataFootballs.Add(football);
            }
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

        public List<string> GetIdEvent(string kod)
        {
            List<string> lisrRez = new List<string>();
            Regex reg = new Regex(@"href.:..en.live.(?<val>.*?).,");
            Regex regNum = new Regex("^[0-9]+$");

            try
            {
                var collection = reg.Matches(kod);

                foreach (Match item in collection)
                {
                    string value = item.Groups["val"].Value;

                    if (regNum.IsMatch(value))
                        lisrRez.Add(value);
                }
            }
            catch { }

            if (lisrRez.Count > 0)
                lisrRez.RemoveAt(0);

            return lisrRez;
        }

        private DataFootball GetInfoFootball(string str)
        {
            DataFootball data = new DataFootball();
            HtmlDocument html = new HtmlDocument();
            string temp = "";

            html.LoadHtml(str);

            temp = html.DocumentNode.SelectSingleNode("//tbody[@class='']").Attributes["data-event-name"].Value;
            data.Team1 = Regex.Split(temp, " vs ")[0];
            data.Team2 = Regex.Split(temp, " vs ")[1];

            data.Liga = html.DocumentNode.SelectSingleNode("//h2[@class='category-label']").InnerText;

            temp = html.DocumentNode.SelectSingleNode("//span[@class='time-description']").InnerText;
            if (temp.Contains("+"))
                data.MinuteMatch = Convert.ToInt32(temp.Split('+')[0]);
            else if (temp.Contains("HT"))
                data.MinuteMatch = 45;
            else
                data.MinuteMatch = Convert.ToInt32(temp.Split(':')[0]);

            temp = html.DocumentNode.SelectSingleNode("//td[@class='event-description']").InnerText;
            string[] arr = temp.Split(':');
            data.SumGoals = Convert.ToInt32(arr[0]);
            if (arr[1].Contains("("))
                data.SumGoals += Convert.ToInt32(arr[1].Split('(')[0]);
            else
                data.SumGoals += Convert.ToInt32(arr[1].Split('\n')[0]);

            return data;
        }

        private void SetKoefFootball(ref DataFootball data, string str)
        {
            Regex regP1 = new Regex("Match_Result.1.\n>(?<val>.*?)<");
            Regex regP2 = new Regex(@"Match_Result.3.\n>(?<val>.*?)<");
            Regex regX = new Regex(@"Match_Result.draw.\n>(?<val>.*?)<");
            Regex regP1X = new Regex("Result.HD.\n>(?<val>.*?)<");
            Regex regP12 = new Regex(@"Result.HA.\n>(?<val>.*?)<");
            Regex regP2X = new Regex(@"Result.AD.\n>(?<val>.*?)<");
            Regex regFora1 = new Regex(data.Team1 + @"..(?<val1>.*?)..,.mn.:.To Win Match With Handicap.,.*?epr.:.(?<val2>.*?).,");
            Regex regFora2 = new Regex(data.Team2 + @"..(?<val1>.*?)..,.mn.:.To Win Match With Handicap.,.*?epr.:.(?<val2>.*?).,");
            Regex regHand1 = new Regex(data.Team1 + @"..(?<val1>.*?)..,.mn.:.To Win Match With Handicap .3 way..,.*?epr.:.(?<val2>.*?).,");
            Regex regHand2 = new Regex(data.Team2 + @"..(?<val1>.*?)..,.mn.:.To Win Match With Handicap .3 way..,.*?epr.:.(?<val2>.*?).,");
            Regex regHandD = new Regex(@"Draw..(?<val1>.*?)..,.mn.:.To Win Match With Handicap .3 way..,.*?epr.:.(?<val2>.*?).,");
            Regex regForaAs1 = new Regex(data.Team1 + @"..(?<val1>.*?)..,.mn.:.To Win Match With Asian Handicap.,.*?epr.:.(?<val2>.*?).,");
            Regex regForaAs2 = new Regex(data.Team2 + @"..(?<val1>.*?)..,.mn.:.To Win Match With Asian Handicap.,.*?epr.:.(?<val2>.*?).,");
            Regex regTotB = new Regex(@"Over (?<val1>.*?).,.mn.:.Total Goals.,.*?epr.:.(?<val2>.*?).,");
            Regex regTotM = new Regex(@"Under (?<val1>.*?).,.mn.:.Total Goals.,.*?epr.:.(?<val2>.*?).,");
            Regex regTot3B = new Regex(@"Over (?<val1>.*?).,.mn.:.Total Goals .3 way..,.*?epr.:.(?<val2>.*?).,");
            Regex regTot3M = new Regex(@"Under (?<val1>.*?).,.mn.:.Total Goals .3 way..,.*?epr.:.(?<val2>.*?).,");
            Regex regTot3E = new Regex(@"Exactly (?<val1>.*?).,.mn.:.Total Goals .3 way..,.*?epr.:.(?<val2>.*?).,");
            Regex regTotAsB = new Regex(@"Over (?<val1>.*?).,.mn.:.Asian Total Goals.,.*?epr.:.(?<val2>.*?).,");
            Regex regTotAsM = new Regex(@"Under (?<val1>.*?).,.mn.:.Asian Total Goals.,.*?epr.:.(?<val2>.*?).,");
            Regex regTotT1B = new Regex(@"Over (?<val1>.*?).,.mn.:.Total Goals ." + data.Team1 + "..,.*?epr.:.(?<val2>.*?).,");
            Regex regTotT1M = new Regex(@"Under (?<val1>.*?).,.mn.:.Total Goals ." + data.Team1 + "..,.*?epr.:.(?<val2>.*?).,");
            Regex regTotT2B = new Regex(@"Over (?<val1>.*?).,.mn.:.Total Goals ." + data.Team2 + "..,.*?epr.:.(?<val2>.*?).,");
            Regex regTotT2M = new Regex(@"Under (?<val1>.*?).,.mn.:.Total Goals ." + data.Team2 + "..,.*?epr.:.(?<val2>.*?).,");

            data.P1 = ConvertToFloat(regP1.Match(str).Groups["val"].Value);
            data.P2 = ConvertToFloat(regP2.Match(str).Groups["val"].Value);
            data.X = ConvertToFloat(regX.Match(str).Groups["val"].Value);
            data.X1 = ConvertToFloat(regP1X.Match(str).Groups["val"].Value);
            data.P12 = ConvertToFloat(regP12.Match(str).Groups["val"].Value);
            data.X2 = ConvertToFloat(regP2X.Match(str).Groups["val"].Value);

            if (str.Contains("\"To Win Match With Handicap\""))
                data.Foras.AddRange(GetFora(str, regFora1, regFora2));
                
            if (str.Contains("\"To Win Match With Handicap (3 way)\""))
                data.Handicaps.AddRange(GetHandicap(str, regHand1, regHand2, regHandD));

            if (str.Contains("\"To Win Match With Asian Handicap\""))
                data.AsiatForas.AddRange(GetFora(str, regForaAs1, regForaAs2));

            if (str.Contains("\"Total Goals\""))
                data.Totals.AddRange(GetTotal(str, regTotB, regTotM));

            if (str.Contains("\"Total Goals (3 way)\""))
                data.Total3Events.AddRange(GetTot3Event(str, regTot3B, regTot3M, regTot3E));

            if (str.Contains("\"Asian Total Goals\""))
                data.AsiatTotals.AddRange(GetTotal(str, regTotAsB, regTotAsM));

            if (str.Contains("\"Total Goals (" + data.Team1 + ")\""))
                data.TotalsK1.AddRange(GetTotal(str, regTotT1B, regTotT1M));

            if (str.Contains("\"Total Goals (" + data.Team2 + ")\""))
                data.TotalsK2.AddRange(GetTotal(str, regTotT2B, regTotT2M));
        }

        #region Methods from Fora
        private List<Fora> GetFora(string str, Regex reg1, Regex reg2)
        {
            List<Fora> list = new List<Fora>();

            var collFora1 = reg1.Matches(str);
            var collFora2 = reg2.Matches(str);

            for (int i = 0; i < collFora1.Count; i++)
            {
                Fora fora = new Fora();

                string nameFora = collFora1[i].Groups["val1"].Value;

                if(nameFora.Contains(","))
                {
                    string[] name = collFora1[i].Groups["val1"].Value.Split(',');

                    if (name[0].Contains("-") || name[1].Contains("-"))
                        fora.Name = Convert.ToString((ConvertToFloat(name[0]) + ConvertToFloat(name[1])) / 2);
                    else
                        fora.Name = "+" + Convert.ToString((ConvertToFloat(name[0]) + ConvertToFloat(name[1])) / 2);
                }
                else
                {
                    fora.Name = nameFora;
                }
                
                fora.Team1 = ConvertToFloat(collFora1[i].Groups["val2"].Value);
                fora.Team2 = GetValueFora(collFora2, collFora1[i].Groups["val1"].Value);

                list.Add(fora);
            }

            return list;
        }

        private List<Handicap> GetHandicap(string str, Regex reg1, Regex reg2, Regex reg3)
        {
            List<Handicap> list = new List<Handicap>();

            var collHand1 = reg1.Matches(str);
            var collHand2 = reg2.Matches(str);
            var collHandD = reg3.Matches(str);

            for (int i = 0; i < collHand1.Count; i++)
            {
                Handicap handicap = new Handicap();
                handicap.Name = collHand1[i].Groups["val1"].Value;
                handicap.Team1 = ConvertToFloat(collHand1[i].Groups["val2"].Value);
                handicap.Team2 = GetValueFora(collHand2, handicap.Name);
                handicap.Draw = ConvertToFloat(collHandD.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(handicap.Name)).Groups["val2"].Value);

                list.Add(handicap);
            }

            return list;
        }

        private float GetValueFora(MatchCollection match, string name1)
        {
            string temp;

            if (name1.Contains("-"))
                temp = name1.Replace("-", "+");
            else
                temp = name1.Replace("+", "-");

            return ConvertToFloat(match.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(temp)).Groups["val2"].Value);
        }

        #endregion

        #region Methods from Total

        private List<Total> GetTotal(string str, Regex reg1, Regex reg2)
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
                    total.Name = temp;

                total.Over = ConvertToFloat(collTotB[i].Groups["val2"].Value);
                total.Under = ConvertToFloat(collTotM.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(temp)).Groups["val2"].Value);

                list.Add(total);
            }

            return list;
        }

        private List<Total3Event> GetTot3Event(string str, Regex reg1, Regex reg2, Regex reg3)
        {
            List<Total3Event> list = new List<Total3Event>();
            var collTotB = reg1.Matches(str);
            var collTotM = reg2.Matches(str);
            var collTotE = reg3.Matches(str);

            for (int i = 0; i < collTotB.Count; i++)
            {
                Total3Event total = new Total3Event();

                total.Name = collTotB[i].Groups["val1"].Value;

                total.Over = ConvertToFloat(collTotB[i].Groups["val2"].Value);
                total.Under = ConvertToFloat(collTotM.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(total.Name)).Groups["val2"].Value);
                total.Exactly = ConvertToFloat(collTotE.Cast<Match>()
                    .FirstOrDefault(x => x.Groups["val1"].Value
                    .Equals(total.Name)).Groups["val2"].Value);

                list.Add(total);
            }
            return list;
        }
        #endregion

        private float ConvertToFloat(string str)
        {
            if (str.Equals(""))
                return 0;

            return Convert.ToSingle(str);
        }
    }
}