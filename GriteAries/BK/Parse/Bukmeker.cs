using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GriteAries.Models;

namespace GriteAries.BK.Parse
{
    public interface Bukmeker
    {
        void ParseLiveFootball();

        List<string> GetIdEvent(string str);


    }
}