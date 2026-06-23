namespace BuyItCommerce.Application.Abstractions.Outbox;

public interface IOutboxWriter
{
    void Add(OutboxMessage message);
}
