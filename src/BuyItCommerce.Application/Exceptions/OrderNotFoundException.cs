namespace BuyItCommerce.Application.Exceptions;

public sealed class OrderNotFoundException : Exception
{
    public Guid OrderId { get; }

    public OrderNotFoundException(Guid orderId)
        : base($"Pedido '{orderId}' não encontrado.")
    {
        OrderId = orderId;
    }
}
