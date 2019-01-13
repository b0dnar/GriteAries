using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GriteAries.Models
{
    public class DataArbitrash
    {
        public string Liga { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public Dictionary<string, TypeBK> ListUrl { get; set; }
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
        public List<Arbitrash> Arbitrashes { get; set; }

        public DataArbitrash()
        {
            ListUrl = new Dictionary<string, TypeBK>();
            Arbitrashes = new List<Arbitrash>();
        }
    }

    public class Arbitrash
    {
        public string Name { get; set; }
        public TypeArbitrash Type { get; set; }
        public ValueBK Koef1 { get; set; }
        public ValueBK Koef2 { get; set; }
        public ValueBK Koef3 { get; set; }
        public int Stavka1 { get; set; }
        public int Stavka2 { get; set; }
        public int Stavka3 { get; set; }
        public float Percent { get; set; }

        public Arbitrash(TypeArbitrash type)
        {
            Type = type;
        }
    }

    public enum TypeArbitrash
    {
        Event2,
        Event3
    }


}