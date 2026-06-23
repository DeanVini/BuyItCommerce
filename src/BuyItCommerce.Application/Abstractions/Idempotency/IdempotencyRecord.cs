namespace BuyItCommerce.Application.Abstractions.Idempotency;

public sealed record IdempotencyRecord(
    string Key,
    string RequestHash,
    string ResponsePayload,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt);
