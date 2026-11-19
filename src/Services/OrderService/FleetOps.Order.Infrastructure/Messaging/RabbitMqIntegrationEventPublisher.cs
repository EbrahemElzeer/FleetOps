using FleetOps.Order.Application.Abstractions.IntegrationEvents;
using FleetOps.Order.Domain.Outbox;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

namespace FleetOps.Order.Infrastructure.Messaging;

public sealed class RabbitMqIntegrationEventPublisher : IIntegrationEventPublisher, IAsyncDisposable
   
{
    private const string ExchangeName = "fleetops.driver";

    private  IConnection? _connection;
    private readonly IConnectionFactory _connectionFactory;
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    private IChannel? _channel;
    private readonly ILogger<RabbitMqIntegrationEventPublisher> _logger;
    public RabbitMqIntegrationEventPublisher(
  
    IConnectionFactory connectionFactory,
    ILogger<RabbitMqIntegrationEventPublisher> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PublishResult>> PublishBatchAsync(
        IReadOnlyList<OutboxMessage> messages,
        CancellationToken cancellationToken = default)
    {
        if (messages.Count == 0)
            return Array.Empty<PublishResult>();

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

            if (anyFailure)
                await ResetChannelAsync();

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
        if (_connection is null || !_connection.IsOpen)
        {
            await ResetConnectionAsync();

            _connection = await _connectionFactory
                .CreateConnectionAsync(ct);
        }

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
        catch (Exception ex)
        {
            _logger.LogWarning(
             ex,
             "Failed to dispose RabbitMQ channel. " +
             "The channel may already be closed or broken.");
        }
    }

    private async Task ResetConnectionAsync()
    {
        await ResetChannelAsync();

        if (_connection is null)
            return;

        var oldConnection = _connection;
        _connection = null;

        try
        {
            await oldConnection.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to dispose RabbitMQ connection.");
        }
    }



    public async ValueTask DisposeAsync()
    {
        await _channelLock.WaitAsync();

        try
        {
            await ResetConnectionAsync();
        }
        finally
        {
            _channelLock.Release();
            _channelLock.Dispose();
        }
    }
}