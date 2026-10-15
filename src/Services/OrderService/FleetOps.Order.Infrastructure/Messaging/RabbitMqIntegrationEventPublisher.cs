using FleetOps.Order.Application.Abstractions.IntegrationEvents;
using FleetOps.Order.Domain.Outbox;
using RabbitMQ.Client;
using System.Text;

namespace FleetOps.Order.Infrastructure.Messaging;

public sealed class RabbitMqIntegrationEventPublisher : IIntegrationEventPublisher, IAsyncDisposable
   
{
    private const string ExchangeName = "fleetops";

    private readonly IConnection _connection;

    private readonly SemaphoreSlim _channelLock = new(1, 1);

    private IChannel? _channel;

    public RabbitMqIntegrationEventPublisher(
        IConnection connection)
    {
        _connection = connection;
    }

    public async Task<IReadOnlyList<PublishResult>> PublishBatchAsync(
        IReadOnlyList<OutboxMessage> messages,
        CancellationToken cancellationToken = default)
    {
        if (messages.Count == 0)
            return Array.Empty<PublishResult>();

        // Only one batch can use the channel at a time.
        await _channelLock.WaitAsync(cancellationToken);

        try
        {
            using var timeoutCts =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);

            timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));

            var ct = timeoutCts.Token;

            var channel = await GetChannelAsync(ct);

            var pending = new List<(
                OutboxMessage Message,
                ValueTask? Confirmation,
                Exception? Error)>(messages.Count);

            // Start publishing without awaiting each confirmation.
            foreach (var message in messages)
            {
                try
                {
                    var properties = new BasicProperties
                    {
                        MessageId = message.Id.ToString(),
                        Type = message.Type,
                        ContentType = "application/json",
                        Persistent = true
                    };

                    var body = Encoding.UTF8.GetBytes(
                        message.Content);

                    var confirmation = channel.BasicPublishAsync(
                        exchange: ExchangeName,
                        routingKey: message.Type,
                        mandatory: true,
                        basicProperties: properties,
                        body: body,
                        cancellationToken: ct);

                    pending.Add((message, confirmation, null));
                }
                catch (Exception ex)
                {
                    pending.Add((message, null, ex));
                }
            }

            var results = new List<PublishResult>(messages.Count);
            var anyFailure = false;

            // Collect confirmation results per message.
            foreach (var item in pending)
            {
                if (item.Error is not null)
                {
                    anyFailure = true;

                    results.Add(new PublishResult(
                        item.Message,
                        false,
                        item.Error.Message));

                    continue;
                }

                try
                {
                    await item.Confirmation!.Value;

                    results.Add(new PublishResult(
                        item.Message,
                        true));
                }
                catch (Exception ex)
                {
                    anyFailure = true;

                    results.Add(new PublishResult(
                        item.Message,
                        false,
                        ex.Message));
                }
            }

            // Never keep an uncertain channel after a failure.
            if (anyFailure)
                await ResetChannelAsync();

            // Application shutdown should propagate.
            cancellationToken.ThrowIfCancellationRequested();

            return results;
        }
        finally
        {
            _channelLock.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync(
        CancellationToken ct)
    {
        if (_channel is { IsOpen: true })
            return _channel;

        await ResetChannelAsync();

        var options = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        _channel = await _connection.CreateChannelAsync(
            options,
            ct);

        return _channel;
    }

    private async Task ResetChannelAsync()
    {
        if (_channel is null)
            return;

        var oldChannel = _channel;
        _channel = null;

        try
        {
            await oldChannel.DisposeAsync();
        }
        catch
        {
            // The channel may already be broken.
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _channelLock.WaitAsync();

        try
        {
            await ResetChannelAsync();
        }
        finally
        {
            _channelLock.Release();
        }

        // Shutdown only: no further PublishBatchAsync calls.
        _channelLock.Dispose();
    }
}