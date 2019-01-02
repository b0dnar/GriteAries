using System;
using System.Collections.Async;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using GriteAries.SystemLogging;
using GriteAries.Models;
using Newtonsoft.Json.Schema;

namespace GriteAries.BK.XBet
{
    public class XBetParse : IBukmekerParse
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

            var kod= await _web.GetPageLiveEvent(typeSport.ToString());
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

            foreach (var item in arrEvent)
            {
                var data = SetDataEvent(item);

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

        private Data SetDataEvent(JToken eventToken)
        {
            Data dataEvent = new Data();

            dataEvent.IdEvent = Convert.ToInt32(eventToken["I"].ToString());
            dataEvent.Liga = eventToken["L"].ToString();
            dataEvent.Team1 = eventToken["O1"].ToString();
            dataEvent.Team2 = eventToken["O2"].ToString();

            var goals = eventToken["SC"]["FS"];
            if(goals.Count() != 0)
            {
                dataEvent.SumGoals = Convert.ToInt32(goals["S1"]?.ToString() ?? "0");
                dataEvent.SumGoals += Convert.ToInt32(goals["S2"]?.ToString() ?? "0");
            }

            int milisec = Convert.ToInt32(eventToken["SC"]["TS"]?.ToString() ?? "0");
            dataEvent.MinuteMatch = milisec / 60;

            return dataEvent;
        }
    }
}