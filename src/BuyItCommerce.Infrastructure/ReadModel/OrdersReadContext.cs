using BuyItCommerce.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BuyItCommerce.Infrastructure.ReadModel;

public sealed class OrdersReadContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoOptions _options;

    public OrdersReadContext(IMongoClient client, IOptions<MongoOptions> options)
    {
        _options = options.Value;
        _database = client.GetDatabase(_options.Database);
    }

    public IMongoCollection<OrderReadModel> Orders =>
        _database.GetCollection<OrderReadModel>(_options.OrdersCollection);
}
