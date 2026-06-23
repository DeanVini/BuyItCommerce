using BuyItCommerce.Application.Abstractions.Caching;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Configuration;
using BuyItCommerce.Application.Orders.Contracts;
using MediatR;
using Microsoft.Extensions.Options;

namespace BuyItCommerce.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(
    IOrderReadRepository orders,
    ICacheService cache,
    IOptions<CacheOptions> cacheOptions) : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    public Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var ttl = TimeSpan.FromSeconds(cacheOptions.Value.ListingTtlSeconds);

        return cache.GetOrCreateAsync(
            $"order:{request.OrderId}",
            ct => orders.GetByIdAsync(request.OrderId, ct),
            ttl,
            cancellationToken);
    }
}
