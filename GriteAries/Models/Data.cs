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
        public string Url { get; set; }
        public int MinuteMatch { get; set; }
        public int SumGoals { get; set; }
        public string Bukmeker { get; set; }
        public ValueBK P1 { get; set; }
        public ValueBK P2 { get; set; }
        public List<Total> Totals { get; set; }
        public List<Fora> Foras { get; set; }

        public Data()
        {
            Totals = new List<Total>();
            Foras = new List<Fora>();
        }
    }
}