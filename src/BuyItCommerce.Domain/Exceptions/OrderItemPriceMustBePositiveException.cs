namespace BuyItCommerce.Domain.Exceptions;

public sealed class OrderItemPriceMustBePositiveException : DomainException
{
    public OrderItemPriceMustBePositiveException(Guid productId)
        : base($"O preço do produto '{productId}' deve ser maior que zero.")
    {
    }
}
