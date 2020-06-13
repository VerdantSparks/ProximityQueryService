using System.Collections.Generic;
using System.Threading.Tasks;
using DataPersistence.NoSql;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace LocationData.MongoAtlas
{
    /// <summary>
    /// We assume your data contains a Geo2DSphere index to work.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MongoAtlasProximityQueryService<T> : IProximityQueryService<T>
    {
        private readonly MongoAtlasService<T> _mongoAtlasService;

        public MongoAtlasProximityQueryService(MongoAtlasService<T> mongoAtlasService)
        {
            _mongoAtlasService = mongoAtlasService;
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

            return geoNearStage;
        }

        public async Task<IEnumerable<ProximityQueryResult<T>>> Nearby(double lon,
                                                                       double lat,
                                                                       ushort radiusInMeters = 1000,
                                                                       ushort limitResultCount = 10,
                                                                       string type = "")
        {
            var collection = _mongoAtlasService.Collection;

            var filter = Builders<T>.Filter.Eq("type", type);
            // Only MongoDb itself has $geoNear aggregate, since .NET driver lack of this one, we need to send the query by writing BsonDocument ourselves

            var pipeline = await collection.Aggregate()
                                           .AppendStage<BsonDocument>(GetGeoNearOptions(lon, lat, radiusInMeters))
                                           .Match(new BsonDocument() {{"type", type}})
                                           .Limit(limitResultCount)
                                           .ToListAsync();

            var results = new List<ProximityQueryResult<T>>();

            //Need to transform back to model object from BsonDocument
            pipeline.ForEach(doc => results.Add(
                                 new ProximityQueryResult<T>(BsonSerializer.Deserialize<T>(doc),
                                                             doc["Distance"].AsDouble)));

            return results;
        }
    }
}
