using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuyItCommerce.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var startedAt = Stopwatch.GetTimestamp();

        logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next().ConfigureAwait(false);
            logger.LogInformation(
                "Handled {RequestName} in {ElapsedMilliseconds} ms",
                requestName,
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            return response;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failure handling {RequestName}", requestName);
            throw;
        }
    }
}
