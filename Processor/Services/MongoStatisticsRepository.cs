using System.Threading.Tasks;
using MongoDB.Driver;
using MonitoringSystem.Shared.Interfaces;
using MonitoringSystem.Shared.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MonitoringSystem.Processor.Services;

public class MongoStatisticsRepository : IStatisticsRepository
{
    private readonly IMongoCollection<ServerStatistics> _collection;

    public MongoStatisticsRepository(string connectionString, string database)
    {
        var client = new MongoClient(connectionString);
        var db = client.GetDatabase(database);
        _collection = db.GetCollection<ServerStatistics>("statistics");
    }

    public async Task InsertAsync(ServerStatistics stats)
    {
        await _collection.InsertOneAsync(stats);
    }

    public async Task<ServerStatistics> GetPreviousAsync(string serverIdentifier)
    {
        var filter = Builders<ServerStatistics>.Filter.Eq(s => s.ServerIdentifier, serverIdentifier);
        var sort = Builders<ServerStatistics>.Sort.Descending(s => s.Timestamp);
        var doc = await _collection.Find(filter).Sort(sort).Skip(1).FirstOrDefaultAsync();
        return doc;
    }
}