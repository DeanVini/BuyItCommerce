using BuyItCommerce.Domain.Orders;

namespace BuyItCommerce.Domain.Exceptions;

public sealed class InvalidOrderTransitionException : DomainException
{
    public InvalidOrderTransitionException(EOrderStatus currentStatus, EOrderStatus targetStatus)
        : base($"Não é possível transicionar um pedido de '{currentStatus}' para '{targetStatus}'.")
    {
    }

    public InvalidOrderTransitionException(EOrderStatus currentStatus, string action)
        : base($"A operação '{action}' não é permitida para um pedido no status '{currentStatus}'.")
    {
    }
}
