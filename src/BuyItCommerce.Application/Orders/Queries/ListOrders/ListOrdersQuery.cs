using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Domain.Orders;
using MediatR;

namespace BuyItCommerce.Application.Orders.Queries.ListOrders;

public sealed record ListOrdersQuery(
    EOrderStatus? Status,
    Guid? BuyerId,
    int Page = 1,
    int PageSize = 20) : IRequest<PagedResult<OrderListItemResponse>>;
