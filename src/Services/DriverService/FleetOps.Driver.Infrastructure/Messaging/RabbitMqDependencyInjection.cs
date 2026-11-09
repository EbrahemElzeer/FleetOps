using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Infrastructure.Messaging
{
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
                    Password = options.Password,

                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval =
                    TimeSpan.FromSeconds(5)
                };
            });
            return services;
        }
    }
}
