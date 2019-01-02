using System.Threading.Tasks;
using GriteAries.BK.Marathone;
using GriteAries.BK.XBet;
using GriteAries.Models;

namespace GriteAries
{
    public class Job
    {
        private MarathoneParse marathone;
        private XBetParse xbet;

        public Job()
        {
            marathone = new MarathoneParse();
            xbet = new XBetParse();
        }
        public async Task RunFootball()
        {
            //var marathoneFootballs = await marathone.GetStartData(TypeSport.Football);
            var xbetFootballs = await xbet.GetStartData(TypeSport.Football);
        }
    }
}