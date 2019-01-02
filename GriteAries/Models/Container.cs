using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GriteAries.Models;

namespace GriteAries.Models
{
    public static class Container
    {
        public static List<int> marathoneUsedIdEvents { get; set; }
        public static List<int> xbetUsedIdEvents{ get; set; }


        static Container()
        {
            marathoneUsedIdEvents = new List<int>();
            xbetUsedIdEvents = new List<int>();
        }

        public static void SetUsedId(TypeBK bk, int idEvent)
        {
            switch(bk)
            {
                case TypeBK.Marathone:
                    marathoneUsedIdEvents.Add(idEvent);
                    break;
                case TypeBK.Xbet:
                    xbetUsedIdEvents.Add(idEvent);
                    break;
                default:
                    break;
            }
        }

        public static List<int> GetUsedId(TypeBK bk)
        {
            switch(bk)
            {
                case TypeBK.Marathone:
                    return marathoneUsedIdEvents;
                case TypeBK.Xbet:
                    return xbetUsedIdEvents;
                default:
                    return null;
            }
        }

        //public static void SetFootball(ref List<Data> data)
        //{
        //    dataAllFootballs.Clear();
        //    dataAllFootballs.AddRange(data);
        //}

        //public static List<int> GetListChoiseId(TypeSport type)
        //{
        //    switch (type)
        //    {
        //        case TypeSport.Football:
        //            return dataChoosenFootballs.Select(x => x.IdEvent).ToList();
        //        default:
        //            return null;
        //    }

        //}

        //public static Data GetDataById(TypeSport type, int id)
        //{
        //    switch(type)
        //    {
        //        case TypeSport.Football:
        //            return  dataAllFootballs.FirstOrDefault(x => x.IdEvent == id);
        //        default:
        //            return null;
        //    }
        //}
        //public static List<Data> GetListData(TypeSport type)
        //{
        //    switch (type)
        //    {
        //        case TypeSport.Football:
        //            return dataAllFootballs;
        //        default:
        //            return null;
        //    }
        //}
    }

    public class UsedData
    {
        public List<Data> footballUsedData;

        public UsedData()
        {
            footballUsedData = new List<Data>();
        }

        public void SetData(TypeSport sport, Data data)
        {
            switch(sport)
            {
                case TypeSport.Football:
                    footballUsedData.Add(data);
                    break;
            }
        }

        public List<Data> GetData(TypeSport sport)
        {
            switch(sport)
            {
                case TypeSport.Football:
                    return footballUsedData;
                default:
                    return null;
            }
        }
    }

}