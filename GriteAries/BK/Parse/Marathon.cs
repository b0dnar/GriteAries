using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using GriteAries.Request;
using GriteAries.Models;
using HtmlAgilityPack;

namespace GriteAries.BK.Parse
{
    public class Marathon : Bukmeker, IBukmeker
    {
        public List<Data> ParseEvent(string[] strCutOut)
        {
            List<Data> list = new List<Data>();
            string urlLive = BaseUrlBK.GetBaseUrl(BaseUrl.MarathonLive);
            string kodPage = Base.HttpGet(urlLive);
            kodPage = CutOutText(kodPage, strCutOut[0], strCutOut[1], strCutOut[2]);

            if (kodPage.Equals(""))
                return list;

            List<int> idEvent = GetIdEvent(kodPage);
            List<int> chooseId = Container.GetListChoiseId(TypeSport.Football);

            foreach (var item in idEvent)
            {
                if (chooseId.Contains(item))
                    continue;

                string urlEvent = BaseUrlBK.GetBaseUrl(BaseUrl.MarathonLive) + item;
                string strEvent = Base.HttpGet(urlEvent);

                Data data = GetInfoEvent(strEvent);

                if (data == null)
                    continue;

                data.Url = urlEvent;
                data.IdEvent = item;

                list.Add(data);
            }

            return list;
        }

        //public void ParseLiveHockey()
        //{
        //    string urlLive = BaseUrlBK.GetBaseUrl(BaseUrl.MarathonLive);
        //    string kodPage = Base.HttpGet(urlLive);
        //    kodPage = CutOutText(kodPage, "sport\",\"label\":\"Ice Hockey", "sport\",\"label\":", "</script>");

        //    if (kodPage.Equals(""))
        //        return;

        //    List<string> idEvent = GetIdEvent(kodPage);
        //    foreach (var item in idEvent)
        //    {
        //        string urlEvent = BaseUrlBK.GetBaseUrl(BaseUrl.MarathonLive) + item;
        //        string strEvent = Base.HttpGet(urlEvent);

        //        Data hockey = GetInfoEvent(strEvent);

        //        if (hockey == null)
        //            continue;

        //        hockey.IdEvent = item;
        //        hockey.Url = urlEvent;
        //        SetKoefHockey(ref hockey, strEvent);

        //        dataHockeys.Add(hockey);
        //    }
        //}

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

        public List<int> GetIdEvent(string kod)
        {
            List<int> lisrRez = new List<int>();
            Regex reg = new Regex(@"href.:..en.live.(?<val>.*?).,");
            Regex regNum = new Regex("^[0-9]+$");

            try
            {
                var collection = reg.Matches(kod);

                foreach (Match item in collection)
                {
                    string value = item.Groups["val"].Value;

                    if (regNum.IsMatch(value))
                        lisrRez.Add(Convert.ToInt32(value));
                }
            }
            catch { }

            if (lisrRez.Count > 0)
                lisrRez.RemoveAt(0);

            return lisrRez;
        }

        private Data GetInfoEvent(string str)
        {
            Data data = new Data();
            HtmlDocument html = new HtmlDocument();
            string temp = "";

            html.LoadHtml(str);

            temp = html.DocumentNode.SelectSingleNode("//tbody[@class='']").Attributes["data-event-name"].Value;
            data.Team1 = Regex.Split(temp, " vs ")[0];
            data.Team2 = Regex.Split(temp, " vs ")[1];

            data.Liga = html.DocumentNode.SelectSingleNode("//h2[@class='category-label']").InnerText;

            try
            {
                temp = html.DocumentNode.SelectSingleNode("//span[@class='time-description']").InnerText;
            }
            catch
            {
                return null;
            }
            
            if (temp.Contains("+"))
                data.MinuteMatch = Convert.ToInt32(temp.Split('+')[0]);
            else if (temp.Contains("HT"))
                data.MinuteMatch = 45;
            else if (temp.Contains("Break"))
                data.MinuteMatch = 20;
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

        private void SetKoefFootball(ref Data data, string str)
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

            data.P1 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP1.Match(str).Groups["val"].Value) };
            data.P2 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP2.Match(str).Groups["val"].Value) }; 
            data.X = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regX.Match(str).Groups["val"].Value) }; 
            data.X1 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP1X.Match(str).Groups["val"].Value) }; 
            data.P12 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP12.Match(str).Groups["val"].Value) };
            data.X2 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP2X.Match(str).Groups["val"].Value) }; 

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

        private void SetKoefHockey(ref Data data, string str)
        {
            Regex regP1 = new Regex("Result.1.\n>(?<val>.*?)<");
            Regex regP2 = new Regex(@"Result.3.\n>(?<val>.*?)<");
            Regex regX = new Regex(@"Result.draw.\n>(?<val>.*?)<");
            Regex regP1X = new Regex("Result0.HD.\n>(?<val>.*?)<");
            Regex regP12 = new Regex(@"Result0.HA.\n>(?<val>.*?)<");
            Regex regP2X = new Regex(@"Result0.AD.\n>(?<val>.*?)<");
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

            data.P1 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP1.Match(str).Groups["val"].Value) }; 
            data.P2 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP2.Match(str).Groups["val"].Value) }; 
            data.X = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regX.Match(str).Groups["val"].Value) }; 
            data.X1 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP1X.Match(str).Groups["val"].Value) }; 
            data.P12 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP12.Match(str).Groups["val"].Value) };
            data.X2 = new ValueBK { BK = TypeBK.Marathone, Value = ConvertToFloat(regP2X.Match(str).Groups["val"].Value) }; 

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
    }
}