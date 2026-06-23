namespace BuyItCommerce.Application.Abstractions.Outbox;

public enum EOutboxEventType
{
    OrderCreated = 0,
    OrderStatusChanged = 1
}
