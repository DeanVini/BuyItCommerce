namespace BuyItCommerce.Domain.Exceptions;

public sealed class OrderMustHaveAtLeastOneItemException : DomainException
{
    public OrderMustHaveAtLeastOneItemException()
        : base("Um pedido deve conter ao menos um produto.")
    {
    }
}
