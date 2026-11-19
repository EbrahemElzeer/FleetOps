using FleetOps.Driver.Application.Drivers.Commands.ProcessReleaseDriverRequested;
using FleetOps.Driver.Application.Drivers.Commands.ReleaseDriver;
using FleetOps.Driver.Domain.Common;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace FleetOps.Driver.Infrastructure.Messaging.Consumers
{
    public sealed class ReleaseDriverRequestedConsumer: BackgroundService
    {
        private const string ExchangeName = "fleetops.driver";
        private const string QueueName = "driver.release-requested";
        private const string RoutingKey = "ReleaseDriverRequested";
        private const string DeadLetterExchangeName = "fleetops.driver.dlx";
        private const string DeadLetterQueueName = "driver.release-requested.dlq";
        private const string DeadLetterRoutingKey = "driver.release-requested.failed";
        private readonly IConnectionFactory _connectionFactory;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ReleaseDriverRequestedConsumer> _logger;
        public ReleaseDriverRequestedConsumer(
        IConnectionFactory connectionFactory,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ReleaseDriverRequestedConsumer> logger)
        {
            _connectionFactory = connectionFactory;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }


        protected override  async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(stoppingToken);
            await using var channel=await connection.CreateChannelAsync(cancellationToken:stoppingToken);

            await channel.ExchangeDeclareAsync(
                exchange: ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await channel.ExchangeDeclareAsync(
                exchange: DeadLetterExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments:new Dictionary<string, object?>
                {
                    ["x-dead-letter-exchange"] = DeadLetterExchangeName,
                    ["x-dead-letter-routing-key"] = DeadLetterRoutingKey
                },
                cancellationToken: stoppingToken);

            await channel.QueueDeclareAsync(
                queue:DeadLetterQueueName,
                durable:true,
                exclusive:false,
                autoDelete:false,
                arguments:null,
                cancellationToken:stoppingToken);

            await channel.QueueBindAsync(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                queue: DeadLetterQueueName,
                exchange: DeadLetterExchangeName,
                routingKey: DeadLetterRoutingKey,
                cancellationToken: stoppingToken);

            _logger.LogInformation(
                 "RabbitMQ topology configured for queue {QueueName}",
                  QueueName);


            var consumer = new AsyncEventingBasicConsumer(channel);


            consumer.ReceivedAsync += async (sender, args) =>
            {
                var body = args.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);


                var message =JsonSerializer.Deserialize<ReleaseDriverRequestedMessage>(json);

                if (message is null)
                {

                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: CancellationToken.None);

                    _logger.LogWarning(
                        "Received an empty ReleaseDriverRequested message.");
                    return;
                }
                if (!Guid.TryParse(
                        args.BasicProperties.MessageId,
                        out var messageId))
                {
                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: CancellationToken.None);

                    _logger.LogWarning(
                        "Invalid RabbitMQ MessageId: {MessageId}",
                        args.BasicProperties.MessageId);

                    return;
                }
                await using var scope = _serviceScopeFactory.CreateAsyncScope();

                var mediator=scope.ServiceProvider.GetRequiredService<IMediator>();


                Result result;

                try
                {
                    result = await mediator.Send(
                        new ProcessReleaseDriverRequestedCommand(
                            message.DriverId,
                            message.OrderId,
                            messageId),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                    when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    // TODO: Implement a bounded retry strategy with delayed retries.
                    // Currently, technical failures are routed to the DLQ without retrying.
                    // Future improvement: Add a RabbitMQ retry queue with TTL, backoff,
                    // and a maximum retry count before dead-lettering permanently.

                    _logger.LogError(
                        ex,
                        "Error processing ReleaseDriverRequested. MessageId: {MessageId}",
                        messageId);

                    if (channel.IsOpen)
                    {
                        await channel.BasicNackAsync(
                            deliveryTag: args.DeliveryTag,
                            multiple: false,
                            requeue: false,
                            cancellationToken: CancellationToken.None);
                    }

                    return;
                }
                if (result.IsFailure)
                {
                    _logger.LogWarning(
                        "Failed to release driver {DriverId}. MessageId: {MessageId}",
                        message.DriverId,
                        messageId);

                    await channel.BasicNackAsync(
                        deliveryTag: args.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: CancellationToken.None);
                    return;
                }
                await channel.BasicAckAsync(
                      deliveryTag: args.DeliveryTag,
                      multiple: false,
                      cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Successfully processed ReleaseDriverRequested. MessageId: {MessageId}",
                    messageId);

               

            };

            await channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);


            try
            {
                await Task.Delay(
                    Timeout.InfiniteTimeSpan,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "ReleaseDriverRequestedConsumer is stopping.");
            }
        }
    }
}
