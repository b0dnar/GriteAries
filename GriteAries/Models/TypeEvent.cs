using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GriteAries.Models
{
    public class Total
    {
        public string Name { get; set; }
        public ValueBK Over { get; set; }
        public ValueBK Under { get; set; }
    }

    public class Total3Event
    {
        public string Name { get; set; }
        public ValueBK Over { get; set; }
        public ValueBK Under { get; set; }
        public ValueBK Exactly { get; set; }
    }

    public class Fora
    {
        public string Name { get; set; }
        public ValueBK Team1 { get; set; }
        public ValueBK Team2 { get; set; }
    }

    public class Handicap
    {
        public string Name { get; set; }
        public ValueBK Team1 { get; set; }
        public ValueBK Draw { get; set; }
        public ValueBK Team2 { get; set; }
    }

    public class ValueBK
    {
        public float Value;
        public TypeBK BK;

        public ValueBK()
        {

        }

        public ValueBK(TypeBK bk)
        {
            BK = bk;
        }

        public ValueBK(TypeBK bk, float val)
        {
            BK = bk;
            Value = val;
        }
    }
}