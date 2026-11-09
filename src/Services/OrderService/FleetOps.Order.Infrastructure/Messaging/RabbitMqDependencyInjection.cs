using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FleetOps.Order.Infrastructure.Messaging;

public static class RabbitMqDependencyInjection
{
    public static IServiceCollection AddRabbitMq(
        this IServiceCollection services)
    {
        services.AddSingleton<IConnectionFactory>(sp =>
        {
            var options = sp
                .GetRequiredService<IOptions<RabbitMqOptions>>()
                .Value;

            return new ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password
            };
        });
        return services;
    }
}