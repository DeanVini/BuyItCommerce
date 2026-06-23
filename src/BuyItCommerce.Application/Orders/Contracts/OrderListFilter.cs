using BuyItCommerce.Domain.Orders;

namespace BuyItCommerce.Application.Orders.Contracts;

public sealed record OrderListFilter(
    EOrderStatus? Status,
    Guid? BuyerId,
    int Page,
    int PageSize);
