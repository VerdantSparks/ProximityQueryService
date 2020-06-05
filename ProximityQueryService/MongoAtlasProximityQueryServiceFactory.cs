namespace ProximityQueryService
{
    public class MongoAtlasProximityQueryServiceFactory
    {
        public MongoAtlasProximityQueryService<T> Create<T>(string a, string b, string c, string d)
        {
            return new MongoAtlasProximityQueryService<T>(a, b, c, d);
        }
    }
}
