using Aco228.Common.Models;
using Aco228.MongoDb.Services;
using MongoDB.Driver;

namespace Aco228.Runners.Consoler.Db;

public interface ILocalDbContext : IMongoDbContext, ISingleton
{
    
}

public class LocalDbContext : MongoDbContext, ILocalDbContext 
{
    public override string DatabaseName => "DummyDatabase";
   
    
    protected override string GetConnectionString()
    {
        var connectionString = Environment.GetEnvironmentVariable("LOCAL_MONGO_CONNECTION");
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException($"Connection string missing");
        return connectionString;
    }
}