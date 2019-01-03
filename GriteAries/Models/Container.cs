using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GriteAries.Models;

namespace GriteAries.Models
{
    public static class Container
    {
        public static List<UsedData> listUsedFootball;
        public static List<int> marathoneUsedIdEvents { get; set; }
        public static List<int> xbetUsedIdEvents{ get; set; }


        static Container()
        {
            listUsedFootball = new List<UsedData>();
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

        public static void SetUsedDatas(TypeSport sport, UsedData data)
        {
            switch (sport)
            {
                case TypeSport.Football:
                    listUsedFootball.Add(data);
                    break;

            }
        }

        public static List<UsedData> GetUsedDatas(TypeSport sport)
        {
            switch (sport)
            {
                case TypeSport.Football:
                    return listUsedFootball;
                default:
                    return null;
            }
        }
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