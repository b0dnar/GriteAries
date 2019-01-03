using System.Collections.Async;
using System.Threading.Tasks;
using System.Collections.Generic;
using GriteAries.BK.Marathone;
using GriteAries.BK.XBet;
using GriteAries.Models;
using GriteAries.Schedulers;
using GriteAries.SystemEquils;
using Quartz;

namespace GriteAries
{
    public class Job
    {
        private MarathoneParse marathone;
        private XBetParse xbet;

        public Job()
        {
            marathone = new MarathoneParse();
            xbet = new XBetParse();
        }

        public async Task RunFootball()
        {
            var marathoneFootballs = await marathone.GetStartData(TypeSport.Football);
            var xbetFootballs = await xbet.GetStartData(TypeSport.Football);

            var listUsedData = Container.GetUsedDatas(TypeSport.Football);

            foreach (var matchMarafon in marathoneFootballs)
            {
                var matchXbet = EquilsMatches(matchMarafon, xbetFootballs);
                if (matchXbet == null)
                {
                    continue;
                }

                Container.SetUsedId(TypeBK.Marathone, matchMarafon.IdEvent);
                Container.SetUsedId(TypeBK.Xbet, matchXbet.IdEvent);

                UsedData usedData = new UsedData();
                usedData.SetData(TypeSport.Football, matchMarafon);
                usedData.SetData(TypeSport.Football, matchXbet);

                listUsedData.Add(usedData);
            }
        }

        private Data EquilsMatches(Data match, List<Data> arrMatches)
        {
            int borderTime = 3;
            double borderKoefLiga = 0.4, borderTeam = 0.7;
            SimilarityTool sEquils = new SimilarityTool();

            foreach (var item in arrMatches)
            {
                if (match.MinuteMatch < item.MinuteMatch - borderTime || match.MinuteMatch > item.MinuteMatch + borderTime)
                    continue;

                if (match.SumGoals != item.SumGoals)
                    continue;

                var kLiga = sEquils.CompareStrings(match.Liga, item.Liga);
                if (kLiga < borderKoefLiga)
                    continue;

                var kTeam1 = sEquils.CompareStrings(match.Team1, item.Team1);
                var kTeam2 = sEquils.CompareStrings(match.Team2, item.Team2);
                if (kTeam1 + kTeam2 < borderTeam)
                    continue;

                return item;
            }

            return null;
        }

        public async Task SetKoef(UsedData usedData)
        {
            int maxThread = 1;
            await usedData.footballUsedData.ParallelForEachAsync(async x =>
            {
                if (x.Bukmeker == TypeBK.Marathone)
                {
                     await marathone.SetKoeficient(x);
                }
                else if (x.Bukmeker == TypeBK.Xbet)
                {
                    
                }
            }, maxThread);
        }
    }
}