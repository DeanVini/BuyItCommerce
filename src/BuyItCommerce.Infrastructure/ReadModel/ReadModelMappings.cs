using BuyItCommerce.Application.Orders.Contracts;

namespace BuyItCommerce.Infrastructure.ReadModel;

internal static class ReadModelMappings
{
    public static OrderResponse ToResponse(this OrderReadModel model) =>
        new(
            model.Id,
            model.BuyerId,
            model.BuyerName,
            model.Status,
            model.TotalAmount,
            model.CreatedAt,
            model.UpdatedAt,
            model.Items.Select(item => item.ToResponse()).ToList());

    public static OrderItemResponse ToResponse(this OrderItemReadModel item) =>
        new(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity, item.LineTotal);

    public static OrderListItemResponse ToListItem(this OrderReadModel model) =>
        new(model.Id, model.BuyerId, model.BuyerName, model.Status, model.TotalAmount, model.CreatedAt);
}
