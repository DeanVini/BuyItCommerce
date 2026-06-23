using System.Text.Json;
using BuyItCommerce.Application.Orders.Contracts;
using BuyItCommerce.Infrastructure.Configuration;
using BuyItCommerce.Infrastructure.Persistence;
using BuyItCommerce.Infrastructure.ReadModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace BuyItCommerce.Infrastructure.Outbox;

internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    OutboxSignal signal,
    IOptions<OutboxOptions> options,
    TimeProvider timeProvider,
    ILogger<OutboxProcessor> logger) : BackgroundService
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly OutboxOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DrainAsync(stoppingToken).ConfigureAwait(false);

        await Task.WhenAll(
            SignalLoopAsync(stoppingToken),
            PollLoopAsync(stoppingToken)).ConfigureAwait(false);
    }

    private async Task SignalLoopAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var _ in signal.Reader.ReadAllAsync(stoppingToken).ConfigureAwait(false))
            {
                await DrainAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task PollLoopAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.PollingIntervalSeconds));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                await DrainAsync(stoppingToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task DrainAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
            var readContext = scope.ServiceProvider.GetRequiredService<OrdersReadContext>();

            var pending = await context.OutboxMessages
                .Where(message => message.ProcessedAt == null)
                .OrderBy(message => message.OccurredAt)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (pending.Count == 0)
            {
                return;
            }

            foreach (var message in pending)
            {
                await ProjectAsync(readContext, message.Payload, cancellationToken).ConfigureAwait(false);
                message.MarkProcessed(timeProvider.GetUtcNow());
            }

            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Falha ao processar mensagens de outbox; serão reprocessadas no próximo ciclo.");
        }
        finally
        {
            _gate.Release();
        }
    }

    private static async Task ProjectAsync(OrdersReadContext readContext, string payload, CancellationToken cancellationToken)
    {
        var response = JsonSerializer.Deserialize<OrderResponse>(payload);
        if (response is null)
        {
            return;
        }

        var readModel = response.ToReadModel();

        await readContext.Orders.ReplaceOneAsync(
            document => document.Id == readModel.Id,
            readModel,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken).ConfigureAwait(false);
    }
}
