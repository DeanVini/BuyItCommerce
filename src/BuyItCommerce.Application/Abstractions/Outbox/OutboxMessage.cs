namespace BuyItCommerce.Application.Abstractions.Outbox;

public sealed class OutboxMessage
{
    public Guid Id { get; private set; }

    public EOutboxEventType Type { get; private set; }

    public string Payload { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    private OutboxMessage(Guid id, EOutboxEventType type, string payload, DateTimeOffset occurredAt)
    {
        Id = id;
        Type = type;
        Payload = payload;
        OccurredAt = occurredAt;
    }

    public static OutboxMessage Create(EOutboxEventType type, string payload, DateTimeOffset occurredAt) =>
        new(Guid.NewGuid(), type, payload, occurredAt);

    public void MarkProcessed(DateTimeOffset processedAt) => ProcessedAt = processedAt;
}
