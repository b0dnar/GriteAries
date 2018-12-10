using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GriteAries.Models
{
    public class DataFootball : Data
    {
        public ValueBK X { get; set; }
        public ValueBK X1 {get;set;}
        public ValueBK X2 { get; set; }
        public ValueBK P12 { get; set; }
        public List<Fora> AsiatForas { get; set; }
        public List<Handicap> Handicaps { get; set; }
        public List<Total> AsiatTotals { get; set; }
        public List<Total> TotalsK1 { get; set; }
        public List<Total> TotalsK2 { get; set; }
        public List<Total3Event> Total3Events { get; set; }

        public DataFootball()
        {
            Initial();
        }

        public DataFootball(Data data)
        {
            Initial();

            Liga = data.Liga;
            Team1 = data.Team1;
            Team2 = data.Team2;
            MinuteMatch = data.MinuteMatch;
            SumGoals = data.SumGoals;
        }

        private void Initial()
        {
            AsiatForas = new List<Fora>();
            Handicaps = new List<Handicap>();
            AsiatTotals = new List<Total>();
            TotalsK1 = new List<Total>();
            TotalsK2 = new List<Total>();
            Total3Events = new List<Total3Event>();
        }
    }
}