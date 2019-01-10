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
                List<DataArbitrash> maxDatas = new List<DataArbitrash>();
                var allFootball = Container.GetUsedDatas(TypeSport.Football);

                foreach (var item in allFootball)
                {
                    var data = GetMaxData(item.footballUsedData);
                    maxDatas.Add(data);
                }

                foreach (var data in maxDatas)
                {
                    SearchArbitrash(data);

                    if(data.Arbitrashes.Count > 0)
                    {

                    }
                }

                
            });
        }

        private DataArbitrash GetMaxData(List<Data> datas)
        {
            DataArbitrash data = new DataArbitrash();

            data.Liga = datas[0].Liga;
            data.Team1 = datas[0].Team1;
            data.Team2 = datas[0].Team2;
            data.ListUrl = datas.ToDictionary(x => x.Url, x => x.Bukmeker);

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

            data.Foras = GetMaxForas(datas.Select(i => i.Foras).ToList());
            data.AsiatForas = GetMaxForas(datas.Select(i => i.AsiatForas).ToList());
            data.Handicaps = GetMaxForas(datas.Select(i => i.Handicaps).ToList());

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

            ValueBK maxValue = null;

            for (int i = 0; i < values.Count; i++)
            {
                if(values[i] == null)
                {
                    continue;
                }

                if(maxValue == null)
                {
                    maxValue = values[i];
                    continue;
                }

                if (maxValue.Value < values[i].Value)
                {
                    maxValue = values[i];
                }
            }

            return maxValue;
        }


        private void SearchArbitrash(DataArbitrash data)
        {
            var arbitr = Arbitrash3Event(data.P1, data.X, data.P2);
            if(arbitr != null)
            {
                arbitr.Name = "P1XP2";
                data.Arbitrashes.Add(arbitr);
            }

            foreach (var item in data.Totals)
            {
                var arb = Arbitrash2Event(item.Over, item.Under);
                if (arb != null)
                {
                    arb.Name = $"Total {item.Name}";
                    data.Arbitrashes.Add(arb);
                }
            }
            foreach (var item in data.TotalsK1)
            {
                var arb = Arbitrash2Event(item.Over, item.Under);
                if (arb != null)
                {
                    arb.Name = $"Total {item.Name} Team1";
                    data.Arbitrashes.Add(arb);
                }
            }
            foreach (var item in data.TotalsK2)
            {
                var arb = Arbitrash2Event(item.Over, item.Under);
                if (arb != null)
                {
                    arb.Name = $"Total {item.Name} Team2";
                    data.Arbitrashes.Add(arb);
                }
            }
            foreach (var item in data.AsiatTotals)
            {
                var arb = Arbitrash2Event(item.Over, item.Under);
                if (arb != null)
                {
                    arb.Name = $"Asiat Total {item.Name}";
                    data.Arbitrashes.Add(arb);
                }
            }
            foreach (var item in data.Total3Events)
            {
                var arb = Arbitrash3Event(item.Over, item.Exactly, item.Under);
                if (arb != null)
                {
                    arb.Name = $"Total {item.Name} Event3";
                    data.Arbitrashes.Add(arb);
                }
            }

            foreach (var item in data.Foras)
            {
                var arb = Arbitrash2Event(item.Team1, item.Team2);
                if (arb != null)
                {
                    arb.Name = $"Fora {item.Name}";
                    data.Arbitrashes.Add(arb);
                }
            }
            foreach (var item in data.AsiatForas)
            {
                var arb = Arbitrash2Event(item.Team1, item.Team2);
                if (arb != null)
                {
                    arb.Name = $"Asian Fora {item.Name}";
                    data.Arbitrashes.Add(arb);
                }
            }
            foreach (var item in data.Handicaps)
            {
                var arb = Arbitrash3Event(item.Team1, item.Draw, item.Team2);
                if(arb != null)
                {
                    arb.Name = $"Handicap {item.Name}";
                    data.Arbitrashes.Add(arb);
                }
            }
        }

        private Arbitrash Arbitrash3Event(ValueBK a1, ValueBK a2, ValueBK a3)
        {
            const int maxProc = 100;
            var sumKoef = maxProc / a1.Value + maxProc / a2.Value + maxProc / a3.Value;

            if (sumKoef >= maxProc)
            {
                return null;
            }

            Arbitrash arbitrash = new Arbitrash(TypeArbitrash.Event3);
            arbitrash.Koef1 = a1;
            arbitrash.Koef2 = a2;
            arbitrash.Koef3 = a3;
            arbitrash.Percent = maxProc - sumKoef;

            return arbitrash;
        }

        private Arbitrash Arbitrash2Event(ValueBK a1, ValueBK a2)
        {
            const int maxProc = 100;
            var sumKoef = maxProc / a1.Value + maxProc / a2.Value;

            if (sumKoef >= maxProc)
            {
                return null;
            }

            Arbitrash arbitrash = new Arbitrash(TypeArbitrash.Event2);
            arbitrash.Koef1 = a1;
            arbitrash.Koef2 = a2;
            arbitrash.Percent = maxProc - sumKoef;

            return arbitrash;
        }

    }
}