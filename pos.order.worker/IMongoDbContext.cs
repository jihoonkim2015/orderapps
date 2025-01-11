using MongoDB.Driver;

namespace pos.wpf.worker
{
    public interface IMongoDbContext
    {
        IMongoDatabase Database { get; }
    }
}
