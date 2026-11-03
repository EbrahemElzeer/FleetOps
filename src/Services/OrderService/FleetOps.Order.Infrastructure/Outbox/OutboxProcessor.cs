using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Abstractions.IntegrationEvents;
using FleetOps.Order.Domain.Outbox;
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
    private readonly Guid _workerId = Guid.NewGuid();
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

        var repository = scope.ServiceProvider
            .GetRequiredService<IOutboxRepository>();

        var publisher = scope.ServiceProvider
            .GetRequiredService<IIntegrationEventPublisher>();

        var messages = await repository.ClaimPendingAsync(
            BatchSize,
            _workerId,
            cancellationToken);

        if (messages.Count == 0)
            return;

        var activeLeases = messages
            .Select(message => new ActiveLease(message))
            .ToList();

        var leasesById = activeLeases.ToDictionary(
            lease => lease.Message.Id);

        using var renewalCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        using var publishingCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        Task? renewalTask = null;
        Task? publishingTask = null;

        try
        {
            renewalTask = RenewLeasesAsync(
                activeLeases,
                renewalCts.Token);

            var publishOperation = publisher.PublishBatchAsync(
                messages,
                publishingCts.Token);

            publishingTask = publishOperation;

            var firstCompleted = await Task.WhenAny(
                publishOperation,
                renewalTask);

            if (firstCompleted == renewalTask)
            {
                publishingCts.Cancel();

                await renewalTask;

                throw new InvalidOperationException(
                    "Lease renewal stopped before publishing completed.");
            }

            var results = await publishOperation;

            if (renewalTask.IsCompleted)
            {
                await renewalTask;

                throw new InvalidOperationException(
                    "Lease renewal stopped unexpectedly.");
            }

            if (results.Count != messages.Count ||
                results.Select(r => r.Message.Id)
                       .Distinct()
                       .Count() != messages.Count ||
                results.Any(r =>
                    !leasesById.ContainsKey(r.Message.Id)))
            {
                throw new InvalidOperationException(
                    "Publisher returned incomplete or invalid results.");
            }

            foreach (var result in results)
            {
                cancellationToken.ThrowIfCancellationRequested();

            
                if (renewalTask.IsCompleted)
                {
                    await renewalTask;

                    throw new InvalidOperationException(
                        "Lease renewal stopped during finalization.");
                }

                var lease = leasesById[result.Message.Id];

                await FinalizeMessageAsync(
                    lease,
                    result,
                    repository,
                    cancellationToken);
            }

            if (renewalTask.IsCompleted)
                await renewalTask;
        }
        finally
        {
            publishingCts.Cancel();
            renewalCts.Cancel();

            if (publishingTask is not null)
            {
                try
                {
                    await publishingTask;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(
                        ex,
                        "Publishing task stopped.");
                }
            }

            if (renewalTask is not null)
            {
                try
                {
                    await renewalTask;
                }
                catch (OperationCanceledException)
                    when (renewalCts.IsCancellationRequested)
                {
                    // Expected shutdown.
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Renewal task stopped with an error.");
                }
            }

            foreach (var lease in activeLeases)
                lease.Dispose();
        }
    }
    private async Task RenewLeasesAsync(IReadOnlyList<ActiveLease> activeLeases,CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(
            TimeSpan.FromSeconds(40));

        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            using var scope = _scopeFactory.CreateScope();

            var repository = scope.ServiceProvider
                .GetRequiredService<IOutboxRepository>();

            foreach (var lease in activeLeases)
            {
                await lease.Gate.WaitAsync(cancellationToken);

                try
                {
                    if (lease.IsCompleted)
                        continue;

                    var message = lease.Message;

                    if (!message.LeaseToken.HasValue)
                        throw new InvalidOperationException(
                            $"Missing LeaseToken: {message.Id}");

                    var renewed = await repository.RenewLeaseAsync(
                        message.Id,
                        _workerId,
                        message.LeaseToken.Value,
                        cancellationToken);

                    if (!renewed)
                    {
                        throw new InvalidOperationException(
                            $"Lease lost for message {message.Id}");
                    }
                }
                finally
                {
                    lease.Gate.Release();
                }
            }

            _logger.LogDebug(
                "Worker {WorkerId} finished lease renewal cycle.",
                _workerId);
        }
    }



    private async Task FinalizeMessageAsync(
    ActiveLease lease,
    PublishResult result,
    IOutboxRepository repository,
    CancellationToken cancellationToken)
    {
        await lease.Gate.WaitAsync(cancellationToken);

        try
        {
            if (lease.IsCompleted)
                return;

            var message = lease.Message;

            if (!message.LeaseToken.HasValue)
                throw new InvalidOperationException(
                    $"Missing LeaseToken for message {message.Id}");

            bool updated;

            if (result.IsConfirmed)
            {
                updated = await repository.TryMarkProcessedAsync(
                    message.Id,
                    _workerId,
                    message.LeaseToken.Value,
                    cancellationToken);
            }
            else
            {
                updated = await repository.TryMarkFailedAsync(
                    message.Id,
                    _workerId,
                    message.LeaseToken.Value,
                    result.Error ?? "RabbitMQ publishing failed.",
                    cancellationToken);
            }

            lease.IsCompleted = true;

            if (!updated)
            {
                _logger.LogWarning(
                    "Message {MessageId} could not be finalized. Lease may have expired or changed.",
                    message.Id);
            }
        }
        finally
        {
            lease.Gate.Release();
        }
    }





    private sealed class ActiveLease : IDisposable
    {
        public OutboxMessage Message { get; }

        public bool IsCompleted { get; set; }

        public SemaphoreSlim Gate { get; } = new(1, 1);

        public ActiveLease(OutboxMessage message)
        {
            Message = message;
        }

        public void Dispose()
        {
            Gate.Dispose();
        }
    }




}

