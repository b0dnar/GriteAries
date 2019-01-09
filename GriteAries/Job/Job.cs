using System.Collections.Async;
using System.Threading.Tasks;
using System.Collections.Generic;
using GriteAries.BK.Marathone;
using GriteAries.BK.XBet;
using GriteAries.Models;
using GriteAries.Schedulers;
using GriteAries.SystemEquils;
using System.Linq;

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
            Dictionary<Data, double> listData = new Dictionary<Data, double>();

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

                var val = kLiga + kTeam1 + kTeam2;
                listData.Add(item, val);
            }

            if(listData.Count == 0)
            {
                return null;
            }
            else if(listData.Count == 1)
            {
                return listData.Keys.ToList()[0];
            }
            else
            {
                var maxVal = listData.Values.ToList().Max();
                return listData.FirstOrDefault(x => x.Value == maxVal).Key;
            }
        }


        public async Task SetKoef(UsedData usedData)
        {
            int maxThread = 10;
            await usedData.footballUsedData.ParallelForEachAsync(async x =>
            {
                switch (x.Bukmeker)
                {
                    case TypeBK.Marathone:
                        await marathone.SetKoeficient(x);
                        break;
                    case TypeBK.Fonbet:
                        break;
                    case TypeBK.Xbet:
                        await xbet.SetKoeficient(x);
                        break;
                    default:
                        break;
                }
            }, maxThread);
        }

        public async Task EquilsData()
        {
            var allFootball = Container.GetUsedDatas(TypeSport.Football);

            foreach (var item in allFootball)
            {
                GetMaxData(item.footballUsedData);
            }
        }

        private Data GetMaxData(List<Data> datas)
        {
            Data data = new Data();

            data.Liga = datas[0].Liga;
            data.Team1 = datas[0].Team1;
            data.Team2 = datas[0].Team2;

            data.P1 = GetMaxValue(datas.Select(i => i.P1).ToList());
            data.X = GetMaxValue(datas.Select(i => i.X).ToList());
            data.P2 = GetMaxValue(datas.Select(i => i.P2).ToList());
            data.X1 = GetMaxValue(datas.Select(i => i.X1).ToList());
            data.P12 = GetMaxValue(datas.Select(i => i.P12).ToList());
            data.X2 = GetMaxValue(datas.Select(i => i.X2).ToList());

            data.Totals = GetMaxTotals(datas.Select(i => i.Totals).ToList());

            return data;
        }

        private List<Total> GetMaxTotals(List<List<Total>> allTotals)
        {
            List<Total> totals = new List<Total>();
            HashSet<string> allName = new HashSet<string>();

            foreach (var item in allTotals)
            {
                var listName = item.Select(x => x.Name);
                foreach (var name in listName)
                {
                    allName.Add(name);
                }
            }

            foreach (var item in allName)//?
            {
                var listTot = allTotals.Where(list => list.Any(s => s.Name == item)).SelectMany(d => d).ToList();
                Total total = new Total();

                total.Name = item;
                total.Over = GetMaxValue(listTot.Select(x => x.Over).ToList());
                total.Under = GetMaxValue(listTot.Select(x => x.Under).ToList());

                totals.Add(total);
            }

            

            return totals;
        }

        private ValueBK GetMaxValue(List<ValueBK> values)
        {
            ValueBK maxValue = values[0];

            for (int i = 1; i < values.Count; i++)
            {
                if (maxValue.Value < values[i].Value)
                {
                    maxValue = values[i];
                }
            }

            return maxValue;
        }
    }
}