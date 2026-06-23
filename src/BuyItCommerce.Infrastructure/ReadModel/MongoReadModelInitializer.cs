using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace BuyItCommerce.Infrastructure.ReadModel;

internal sealed class MongoReadModelInitializer(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrdersReadContext>();

        var builder = Builders<OrderReadModel>.IndexKeys;
        var indexes = new[]
        {
            new CreateIndexModel<OrderReadModel>(builder.Ascending(order => order.BuyerId)),
            new CreateIndexModel<OrderReadModel>(builder.Ascending(order => order.Status)),
            new CreateIndexModel<OrderReadModel>(builder.Descending(order => order.CreatedAt))
        };

        await context.Orders.Indexes.CreateManyAsync(indexes, cancellationToken).ConfigureAwait(false);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
