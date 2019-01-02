using System.Threading.Tasks;
using System.Collections.Generic;
using GriteAries.Models;

namespace GriteAries.BK
{
    public interface IBukmekerParse
    {
        Task<List<Data>> GetStartData(TypeSport typeSport);
    }
}
