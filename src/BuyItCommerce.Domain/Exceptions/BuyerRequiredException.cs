namespace BuyItCommerce.Domain.Exceptions;

public sealed class BuyerRequiredException : DomainException
{
    public BuyerRequiredException()
        : base("Um pedido deve ter um comprador com identificador e nome válidos.")
    {
    }
}
