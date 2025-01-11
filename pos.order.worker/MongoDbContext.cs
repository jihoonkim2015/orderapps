using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace pos.wpf.worker
{
    public class MongoDbContext : IMongoDbContext
    {
        public IMongoDatabase Database { get; }

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration["MongoDB:ConnectionString"];
            var databaseName = configuration["MongoDB:DatabaseName"];
            var client = new MongoClient(connectionString);
            Database = client.GetDatabase(databaseName);
        }
    }
}
