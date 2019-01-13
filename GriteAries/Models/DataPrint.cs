using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GriteAries.Models
{
    public class DataPrint
    {
        public bool State2Event { get; set; }
        public float Percent { get; set; }
        public string NameArbitrash { get; set; }
        public ValueBK Koef1 { get; set; }
        public ValueBK Koef2 { get; set; }
        public ValueBK Koef3 { get; set; }
        public int Stavka1 { get; set; }
        public int Stavka2 { get; set; }
        public int Stavka3 { get; set; }
        public string Liga { get; set; }
        public string NameMatches { get; set; }
        public List<string> Urls { get; set; }
        public Dictionary<TypeBK, string> bkToString { get; set; }

        public DataPrint()
        {
            bkToString = new Dictionary<TypeBK, string>();
            bkToString.Add(TypeBK.Marathone, "Marathone");
            bkToString.Add(TypeBK.Xbet, "Xbet");

        }
    }
}