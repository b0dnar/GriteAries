using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GriteAries.Models
{
    public class Data
    {
        public string Liga { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public int IdEvent { get; set; }
        public string Url { get; set; }
        public int MinuteMatch { get; set; }
        public int SumGoals { get; set; }
        public TypeBK Bukmeker { get; set; }
        public ValueBK P1 { get; set; }
        public ValueBK P2 { get; set; }
        public ValueBK X { get; set; }
        public ValueBK X1 { get; set; }
        public ValueBK X2 { get; set; }
        public ValueBK P12 { get; set; }
        public List<Total> Totals { get; set; }
        public List<Total> AsiatTotals { get; set; }
        public List<Total> TotalsK1 { get; set; }
        public List<Total> TotalsK2 { get; set; }
        public List<Total3Event> Total3Events { get; set; }
        public List<Fora> Foras { get; set; }
        public List<Fora> AsiatForas { get; set; }
        public List<Handicap> Handicaps { get; set; }

        public Data()
        {
            Totals = new List<Total>();
            AsiatTotals = new List<Total>();
            TotalsK1 = new List<Total>();
            TotalsK2 = new List<Total>();
            Total3Events = new List<Total3Event>();
            Foras = new List<Fora>();
            AsiatForas = new List<Fora>();
            Handicaps = new List<Handicap>();
        }
    }
}



