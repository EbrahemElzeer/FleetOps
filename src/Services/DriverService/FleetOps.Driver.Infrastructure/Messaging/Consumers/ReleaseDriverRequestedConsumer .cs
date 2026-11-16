using FleetOps.Driver.Application.Drivers.Commands.ReleaseDriver;
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
        private const string ExchangeName = "fleetops";
        private const string QueueName = "driver.release-requested";
        private const string RoutingKey = "ReleaseDriverRequested";
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

            await channel.QueueDeclareAsync(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: RoutingKey,
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
                    _logger.LogWarning(
                        "Received an empty ReleaseDriverRequested message.");

                    return;
                }

                await using var scope = _serviceScopeFactory.CreateAsyncScope();

                var mediator=scope.ServiceProvider.GetRequiredService<IMediator>();

                var result= await mediator.Send(new ReleaseDriverCommand(message.DriverId,message.OrderId),
                    stoppingToken);

                if (result.IsFailure)
                {
                    _logger.LogWarning(
                        "Failed to release driver {DriverId}",
                        message.DriverId);

                    return;
                }

                await channel.BasicAckAsync(
                    deliveryTag: args.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);

                _logger.LogInformation(
                    "Received message: {MessageId}",
                    args.BasicProperties.MessageId);

                await Task.CompletedTask;
            };

            await channel.BasicConsumeAsync(
                queue: QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

        }
    }
}
