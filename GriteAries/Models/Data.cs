using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GriteAries.Models
{
    public abstract class Data
    {
        public string Liga { get; set; }
        public string Team1 { get; set; }
        public string Team2 { get; set; }
        public string Url { get; set; }
        public int MinuteMatch { get; set; }
        public int SumGoals { get; set; }
        public string Bukmeker { get; set; }
        public float P1 { get; set; }
        public float P2 { get; set; }
        public List<Total> Totals { get; set; }
        public List<Fora> Foras { get; set; }

        public Data()
        {
            Totals = new List<Total>();
            Foras = new List<Fora>();
        }
    }

    public abstract class OutputData
    {
        public List<DataFootball> dataFootballs { get; set; }
        public List<DataHockey> dataHockeys { get; set; }

        public OutputData()
        {
            dataFootballs = new List<DataFootball>();
            dataHockeys = new List<DataHockey>();
        }
    }

    public class Total
    {
        public string Name { get; set; }
        public float Over { get; set; }
        public float Under { get; set; }
    }

    public class Total3Event
    {
        public string Name { get; set; }
        public float Over { get; set; }
        public float Under { get; set; }
        public float Exactly { get; set; }
    }

    public class Fora
    {
        public string Name { get; set; }
        public float Team1 { get; set; }
        public float Team2 { get; set; }
    }

    public class Handicap
    {
        public string Name { get; set; }
        public float Team1 { get; set; }
        public float Draw { get; set; }
        public float Team2 { get; set; }
    }
}