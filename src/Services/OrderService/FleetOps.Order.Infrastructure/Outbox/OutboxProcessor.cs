using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Abstractions.IntegrationEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FleetOps.Order.Infrastructure.Outbox;

public sealed class OutboxProcessor : BackgroundService
{
    private const int BatchSize = 20;

    private static readonly TimeSpan PollingInterval =
        TimeSpan.FromSeconds(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMessagesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while processing outbox messages.");
            }

            try
            {
                await Task.Delay(
                    PollingInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcessMessagesAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var outboxRepository =
            scope.ServiceProvider
                .GetRequiredService<IOutboxRepository>();

        var publisher =
            scope.ServiceProvider
                .GetRequiredService<IIntegrationEventPublisher>();

        var unitOfWork =
            scope.ServiceProvider
                .GetRequiredService<IUnitOfWork>();

        var messages = await outboxRepository.GetPendingAsync(
            BatchSize,
            cancellationToken);

        if (messages.Count == 0)
            return;

        var results = await publisher.PublishBatchAsync(
            messages,
            cancellationToken);

        foreach (var result in results)
        {
            if (result.IsConfirmed)
            {
                result.Message.MarkProcessed();

                _logger.LogInformation(
                    "Outbox message {MessageId} published successfully.",
                    result.Message.Id);
            }
            else
            {
                result.Message.MarkFailed(
                    result.Error ?? "RabbitMQ publishing failed.");

                _logger.LogWarning(
                    "Failed to publish outbox message {MessageId}. RetryCount: {RetryCount}. Error: {Error}",
                    result.Message.Id,
                    result.Message.RetryCount,
                    result.Error);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}