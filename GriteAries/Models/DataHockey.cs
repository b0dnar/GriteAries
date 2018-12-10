using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GriteAries.Models
{
    public class DataHockey : Data
    {
        public float X { get; set; }
        public float X1 { get; set; }
        public float X2 { get; set; }
        public float P12 { get; set; }
        public List<Total> TotalsK1 { get; set; }
        public List<Total> TotalsK2 { get; set; }

        public DataHockey()
        {
            TotalsK1 = new List<Total>();
            TotalsK2 = new List<Total>();
        }
    }
}