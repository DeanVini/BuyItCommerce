using BuyItCommerce.Domain.Exceptions;

namespace BuyItCommerce.Domain.Orders;

public sealed record OrderItem
{
    public Guid ProductId { get; }

    public string ProductName { get; }

    public decimal UnitPrice { get; }

    public int Quantity { get; }

    public decimal LineTotal => UnitPrice * Quantity;

    private OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public static OrderItem Create(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (unitPrice <= 0)
        {
            throw new OrderItemPriceMustBePositiveException(productId);
        }

        if (quantity <= 0)
        {
            throw new OrderItemQuantityMustBePositiveException(productId);
        }

        return new OrderItem(productId, productName?.Trim() ?? string.Empty, unitPrice, quantity);
    }
}
