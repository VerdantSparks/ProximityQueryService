namespace ProximityQueryService.MongoAtlas
{
    public class MongoAtlasProximityQueryServiceFactory
    {
        public MongoAtlasProximityQueryService<T> Create<T>(string endpoint, string user, string password, string database)
        {
            return new MongoAtlasProximityQueryService<T>(endpoint, user, password, database);
        }
    }
}
