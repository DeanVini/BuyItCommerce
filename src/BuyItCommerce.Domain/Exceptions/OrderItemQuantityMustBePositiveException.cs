namespace BuyItCommerce.Domain.Exceptions;

public sealed class OrderItemQuantityMustBePositiveException : DomainException
{
    public OrderItemQuantityMustBePositiveException(Guid productId)
        : base($"A quantidade do produto '{productId}' deve ser maior que zero.")
    {
    }
}
