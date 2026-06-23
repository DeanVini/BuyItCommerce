namespace BuyItCommerce.Application.Exceptions;

public sealed class IdempotencyConflictException : Exception
{
    public string IdempotencyKey { get; }

    public IdempotencyConflictException(string idempotencyKey)
        : base($"A chave de idempotência '{idempotencyKey}' já foi usada com um conteúdo diferente.")
    {
        IdempotencyKey = idempotencyKey;
    }
}
