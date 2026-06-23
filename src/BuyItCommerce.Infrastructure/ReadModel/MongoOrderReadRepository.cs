using BuyItCommerce.Application.Abstractions.Persistence;
using BuyItCommerce.Application.Orders.Contracts;
using MongoDB.Driver;

namespace BuyItCommerce.Infrastructure.ReadModel;

internal sealed class MongoOrderReadRepository(OrdersReadContext context) : IOrderReadRepository
{
    public async Task<OrderResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await context.Orders
            .Find(order => order.Id == id)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return document?.ToResponse();
    }

    public async Task<PagedResult<OrderListItemResponse>> ListAsync(OrderListFilter filter, CancellationToken cancellationToken)
    {
        var builder = Builders<OrderReadModel>.Filter;
        var conditions = new List<FilterDefinition<OrderReadModel>>();

        if (filter.Status is not null)
        {
            conditions.Add(builder.Eq(order => order.Status, filter.Status.Value.ToString()));
        }

        if (filter.BuyerId is not null)
        {
            conditions.Add(builder.Eq(order => order.BuyerId, filter.BuyerId.Value));
        }

        var query = conditions.Count > 0 ? builder.And(conditions) : builder.Empty;

        var totalCount = await context.Orders
            .CountDocumentsAsync(query, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var documents = await context.Orders
            .Find(query)
            .SortByDescending(order => order.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Limit(filter.PageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        var items = documents.Select(document => document.ToListItem()).ToList();

        return new PagedResult<OrderListItemResponse>(items, filter.Page, filter.PageSize, totalCount);
    }
}
