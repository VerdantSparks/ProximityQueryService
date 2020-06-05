using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProximityQueryService
{
    public interface IProximityQueryService<T>
    {
        Task<IEnumerable<ProximityQueryResult<T>>> Nearby(double lon,
                                                          double lat,
                                                          uint radiusInMeters,
                                                          string type,
                                                          ushort limitResultCount = 10);
    }
}
