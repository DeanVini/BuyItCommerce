namespace BuyItCommerce.Infrastructure.Outbox;

public interface IOutboxSignal
{
    void Notify(Guid messageId);
}
