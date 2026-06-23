using System.Text.Json;
using BuyItCommerce.Application.Abstractions.Idempotency;
using BuyItCommerce.Application.Exceptions;
using MediatR;

namespace BuyItCommerce.Application.Behaviors;

public sealed class IdempotencyBehavior<TRequest, TResponse>(
    IIdempotencyStore store)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IIdempotentRequest idempotentRequest
            || string.IsNullOrWhiteSpace(idempotentRequest.IdempotencyKey))
        {
            return await next().ConfigureAwait(false);
        }

        var existing = await store
            .FindAsync(idempotentRequest.IdempotencyKey, cancellationToken)
            .ConfigureAwait(false);

        if (existing is null)
        {
            return await next().ConfigureAwait(false);
        }

        var currentHash = RequestHashGenerator.Compute(request);
        if (!string.Equals(existing.RequestHash, currentHash, StringComparison.Ordinal))
        {
            throw new IdempotencyConflictException(idempotentRequest.IdempotencyKey);
        }

        var replay = JsonSerializer.Deserialize<TResponse>(existing.ResponsePayload);
        return replay ?? await next().ConfigureAwait(false);
    }
}
