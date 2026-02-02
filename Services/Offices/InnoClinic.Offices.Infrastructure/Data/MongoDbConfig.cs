using InnoClinic.Offices.Domain.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

public static class MongoDbConfig
{
    public static void Configure()
    {
        BsonClassMap.RegisterClassMap<Office>(cm =>
        {
            cm.AutoMap();

            cm.MapIdProperty(c => c.Id)
                .SetIdGenerator(StringObjectIdGenerator.Instance)
                .SetSerializer(new StringSerializer(BsonType.ObjectId));
        });
    }
}