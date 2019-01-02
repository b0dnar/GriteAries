using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GriteAries.Models
{
    public static class Container
    {
        public static List<Data> dataAllFootballs { get; set; }
        public static List<Data> dataChoosenFootballs { get; set; }

        static Container()
        {
            dataAllFootballs = new List<Data>();
            dataChoosenFootballs = new List<Data>();
        }

        public static void SetFootball(ref List<Data> data)
        {
            dataAllFootballs.Clear();
            dataAllFootballs.AddRange(data);
        }

        public static List<int> GetListChoiseId(TypeSport type)
        {
            switch (type)
            {
                case TypeSport.Football:
                    return dataChoosenFootballs.Select(x => x.IdEvent).ToList();
                default:
                    return null;
            }

        }

        public static Data GetDataById(TypeSport type, int id)
        {
            switch(type)
            {
                case TypeSport.Football:
                    return  dataAllFootballs.FirstOrDefault(x => x.IdEvent == id);
                default:
                    return null;
            }
        }
        public static List<Data> GetListData(TypeSport type)
        {
            switch (type)
            {
                case TypeSport.Football:
                    return dataAllFootballs;
                default:
                    return null;
            }
        }
    }
}