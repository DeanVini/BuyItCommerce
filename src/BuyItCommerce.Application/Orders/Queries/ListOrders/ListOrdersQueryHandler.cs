using BuyItCommerce.Application.Abstractions.Caching;
using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Configuration;
using BuyItCommerce.Application.Orders.Contracts;
using MediatR;
using Microsoft.Extensions.Options;

namespace BuyItCommerce.Application.Orders.Queries.ListOrders;

public sealed class ListOrdersQueryHandler(
    IOrderReadRepository orders,
    ICacheService cache,
    IOptions<CacheOptions> cacheOptions) : IRequestHandler<ListOrdersQuery, PagedResult<OrderListItemResponse>>
{
    public Task<PagedResult<OrderListItemResponse>> Handle(ListOrdersQuery request, CancellationToken cancellationToken)
    {
        var filter = new OrderListFilter(request.Status, request.BuyerId, request.Page, request.PageSize);
        var ttl = TimeSpan.FromSeconds(cacheOptions.Value.ListingTtlSeconds);
        var cacheKey = $"orders:{request.Status}:{request.BuyerId}:{request.Page}:{request.PageSize}";

        return cache.GetOrCreateAsync(
            cacheKey,
            ct => orders.ListAsync(filter, ct),
            ttl,
            cancellationToken);
    }
}
