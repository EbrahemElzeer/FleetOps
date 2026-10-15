using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FleetOps.Order.Infrastructure.Messaging;

public static class RabbitMqDependencyInjection
{
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services)
    {
        services.AddSingleton<IConnection>(sp =>
        {
            var options = sp
               .GetRequiredService<IOptions<RabbitMqOptions>>()
               .Value;

            var factory = new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password
            };


            return factory
                .CreateConnectionAsync()
                .GetAwaiter()
                .GetResult();
        });

        return services;
    }
}