using System;
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
            await Task.Run(() =>
            {
                List<Data> maxDatas = new List<Data>();
                var allFootball = Container.GetUsedDatas(TypeSport.Football);

                foreach (var item in allFootball)
                {
                    var data = GetMaxData(item.footballUsedData);
                    maxDatas.Add(data);
                }

                foreach (var data in maxDatas)
                {
                    SearchArbitrash(data);
                }
            });
        }

        private Data GetMaxData(List<Data> datas)
        {
            Data data = new Data();

            data.Liga = $"{datas[0].Liga}\t{datas[1].Liga}";
            data.Team1 = $"{datas[0].Team1}\t{datas[1].Team1}";
            data.Team2 = $"{datas[0].Team2}\t{datas[1].Team2}";

            data.P1 = GetMaxValue(datas.Select(i => i.P1).ToList());
            data.X = GetMaxValue(datas.Select(i => i.X).ToList());
            data.P2 = GetMaxValue(datas.Select(i => i.P2).ToList());
            data.X1 = GetMaxValue(datas.Select(i => i.X1).ToList());
            data.P12 = GetMaxValue(datas.Select(i => i.P12).ToList());
            data.X2 = GetMaxValue(datas.Select(i => i.X2).ToList());

            data.Totals = GetMaxTotals(datas.Select(i => i.Totals).ToList());
            data.TotalsK1 = GetMaxTotals(datas.Select(i => i.TotalsK1).ToList());
            data.TotalsK2 = GetMaxTotals(datas.Select(i => i.TotalsK2).ToList());
            data.AsiatTotals = GetMaxTotals(datas.Select(i => i.AsiatTotals).ToList());
            data.Total3Events = GetMaxTotals(datas.Select(i => i.Total3Events).ToList());

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

            foreach (var item in allName)
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

        private List<Total3Event> GetMaxTotals(List<List<Total3Event>> allTotals)
        {
            List<Total3Event> totals = new List<Total3Event>();
            HashSet<string> allName = new HashSet<string>();

            foreach (var item in allTotals)
            {
                var listName = item.Select(x => x.Name);
                foreach (var name in listName)
                {
                    allName.Add(name);
                }
            }

            foreach (var item in allName)
            {
                var listTot = allTotals.Where(list => list.Any(s => s.Name == item)).SelectMany(d => d).ToList();
                Total3Event total = new Total3Event();

                total.Name = item;
                total.Over = GetMaxValue(listTot.Select(x => x.Over).ToList());
                total.Under = GetMaxValue(listTot.Select(x => x.Under).ToList());
                total.Exactly = GetMaxValue(listTot.Select(x => x.Exactly).ToList());

                totals.Add(total);
            }

            return totals;
        }

        private List<Fora> GetMaxForas(List<List<Fora>> allForas)
        {
            List<Fora> foras = new List<Fora>();
            HashSet<string> allName = new HashSet<string>();

            foreach (var item in allForas)
            {
                var listName = item.Select(x => x.Name);
                foreach (var name in listName)
                {
                    allName.Add(name);
                }
            }

            foreach (var item in allName)
            {
                var listTot = allForas.Where(list => list.Any(s => s.Name == item)).SelectMany(d => d).ToList();
                Fora fora = new Fora();

                fora.Name = item;
                fora.Team1 = GetMaxValue(listTot.Select(x => x.Team1).ToList());
                fora.Team2 = GetMaxValue(listTot.Select(x => x.Team2).ToList());

                foras.Add(fora);
            }

            return foras;
        }

        private List<Handicap> GetMaxForas(List<List<Handicap>> allForas)
        {
            List<Handicap> foras = new List<Handicap>();
            HashSet<string> allName = new HashSet<string>();

            foreach (var item in allForas)
            {
                var listName = item.Select(x => x.Name);
                foreach (var name in listName)
                {
                    allName.Add(name);
                }
            }

            foreach (var item in allName)
            {
                var listTot = allForas.Where(list => list.Any(s => s.Name == item)).SelectMany(d => d).ToList();
                Handicap fora = new Handicap();

                fora.Name = item;
                fora.Team1 = GetMaxValue(listTot.Select(x => x.Team1).ToList());
                fora.Team2 = GetMaxValue(listTot.Select(x => x.Team2).ToList());
                fora.Draw = GetMaxValue(listTot.Select(x => x.Draw).ToList());

                foras.Add(fora);
            }

            return foras;
        }

        private ValueBK GetMaxValue(List<ValueBK> values)
        {
            if (values.Count == 0)
            {
                return new ValueBK(bk:TypeBK.Xbet);
            }

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


        private void SearchArbitrash(Data data)
        {
            string comment = $"{data.Liga}\t{data.Team1}\t{data.Team2}";

            Arbitrash3Event(data.P1.Value, data.X.Value, data.P2.Value, $"{comment}\tP1XP2");

            foreach (var item in data.Totals)
            {
                Arbitrash2Event(item.Over.Value, item.Under.Value, $"{comment}\ttotal {item.Name}");
            }
            foreach (var item in data.TotalsK1)
            {
                Arbitrash2Event(item.Over.Value, item.Under.Value, $"{comment}\ttotal team1 {item.Name}");
            }
            foreach (var item in data.TotalsK2)
            {
                Arbitrash2Event(item.Over.Value, item.Under.Value, $"{comment}\ttotal team2 {item.Name}");
            }
            foreach (var item in data.AsiatTotals)
            {
                Arbitrash2Event(item.Over.Value, item.Under.Value, $"{comment}\tasiat total {item.Name}");
            }
            foreach (var item in data.Total3Events)
            {
                Arbitrash3Event(item.Over.Value, item.Under.Value, item.Exactly.Value, $"{comment}\ttotal 3 event {item.Name}");
            }

            foreach (var item in data.Foras)
            {
                Arbitrash2Event(item.Team1.Value, item.Team2.Value, $"{comment}\fora {item.Name}");
            }
            foreach (var item in data.AsiatForas)
            {
                Arbitrash2Event(item.Team1.Value, item.Team2.Value, $"{comment}\tasiat fora {item.Name}");
            }
            foreach (var item in data.Handicaps)
            {
                Arbitrash3Event(item.Team1.Value, item.Team2.Value, item.Draw.Value, $"{comment}\thandicap {item.Name}");
            }
        }

        private void Arbitrash3Event(float a1, float a2, float a3, string comment)
        {
            const int maxProc = 100;
            var sumKoef = maxProc / a1 + maxProc / a2 + maxProc / a3;

            if (sumKoef >= maxProc)
            {
                return;
            }

            Console.WriteLine(  );
        }

        private void Arbitrash2Event(float a1, float a2, string comment)
        {
            const int maxProc = 100;
            var sumKoef = maxProc / a1 + maxProc / a2;

            if (sumKoef >= maxProc)
            {
                return;
            }

            Console.WriteLine(  );
        }

    }
}