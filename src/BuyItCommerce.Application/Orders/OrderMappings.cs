using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Domain.Orders;

namespace BuyItCommerce.Application.Orders;

public static class OrderMappings
{
    public static OrderResponse ToResponse(this Order order) =>
        new(
            order.Id,
            order.Buyer.Id,
            order.Buyer.Name,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt,
            order.Items.Select(item => item.ToResponse()).ToList());

    public static OrderItemResponse ToResponse(this OrderItem item) =>
        new(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity, item.LineTotal);

    public static IEnumerable<OrderItem> ToDomainItems(this IEnumerable<OrderItemInput> items) =>
        items.Select(item => OrderItem.Create(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity));
}
