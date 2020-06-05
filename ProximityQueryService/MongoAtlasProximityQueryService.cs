using System.Collections.Generic;
using System.Security.Authentication;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ProximityQueryService
{
    public class MongoAtlasProximityQueryService<T> : IProximityQueryService<T>
    {
        private const string BaseConnStr = "mongodb+srv://{0}:{1}@{2}/{3}?retryWrites=true&w=majority";
        private readonly MongoClient _mongoClient;

        private readonly string _databaseName;

        public MongoAtlasProximityQueryService(string endpoint, string user, string password, string database)
        {
            _databaseName = database;
            _mongoClient = new MongoClient(GetMongoSetting(GetMongoConnStr(endpoint, user, password, database)));
        }

        private static string GetMongoConnStr(string endpoint, string user, string password, string database)
        {
            return string.Format(BaseConnStr, user, UrlEncoder.Create().Encode(password), endpoint, database);
        }

        private static MongoClientSettings GetMongoSetting(string connStr)
        {
            var settings = MongoClientSettings.FromUrl(new MongoUrl(connStr));
            settings.SslSettings = new SslSettings() {EnabledSslProtocols = SslProtocols.Tls12};

            return settings;
        }

        private static BsonDocument GetGeoNearOptions(double lat, double lon, uint radiusInMeters)
        {
            var geoNearOptions = new BsonDocument()
            {
                {"near", new BsonDocument() {{"type", "Point"}, {"coordinates", new BsonArray {lon, lat}}}},
                {"distanceField", "Distance"},
                {"maxDistance", radiusInMeters}
            };

            var geoNearStage = new BsonDocument() {{"$geoNear", geoNearOptions}};

            return geoNearOptions;
        }

        public async Task<IEnumerable<ProximityQueryResult<T>>> Nearby(double lon,
                                                                       double lat,
                                                                       uint radiusInMeters,
                                                                       string type,
                                                                       ushort limitResultCount = 10)
        {
            var db = _mongoClient.GetDatabase(_databaseName);
            var collection = db.GetCollection<T>(type);

            // Only MongoDb itself has $geoNear aggregate, since .NET driver lack of this one, we need to send the query by writing BsonDocument ourselves

            var pipeline = await collection.Aggregate()
                                           .AppendStage<BsonDocument>(GetGeoNearOptions(lon, lat, radiusInMeters))
                                           .Limit(limitResultCount)
                                           .ToListAsync();

            var results = new List<ProximityQueryResult<T>>();

            pipeline.ForEach(doc => results.Add(
                                 new ProximityQueryResult<T>(BsonSerializer.Deserialize<T>(doc),
                                                             doc["Distance"].AsDouble)));

            //Need to transform back to model object from BsonDocument

            return results;
        }
    }
}
