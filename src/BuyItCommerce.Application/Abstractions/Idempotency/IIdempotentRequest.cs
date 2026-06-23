namespace BuyItCommerce.Application.Abstractions.Idempotency;

public interface IIdempotentRequest
{
    string IdempotencyKey { get; }
}
