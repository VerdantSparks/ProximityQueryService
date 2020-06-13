using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocationData
{
    public interface IProximityQueryService<T>
    {
        Task<IEnumerable<ProximityQueryResult<T>>> Nearby(double lon,
                                                          double lat,
                                                          ushort radiusInMeters,
                                                          ushort limitResultCount,
                                                          string type);
    }
}
