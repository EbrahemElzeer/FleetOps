using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Infrastructure.ExternalServices;
using FleetOps.Order.Infrastructure.Messaging;
using FleetOps.Order.Infrastructure.Persistence;
using FleetOps.Order.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace FleetOps.Order.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<OrderDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("OrderDatabase"));
        });

        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAssignDriverSagaRepository,AssignDriverSagaRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.Configure<DriverServiceOptions>(
        configuration.GetSection(DriverServiceOptions.SectionName));
        services.AddRabbitMq();
        services.AddHttpClient<IDriverReservationService, DriverReservationService>(
            (serviceProvider, client) =>
            {
                var options = serviceProvider
                    .GetRequiredService<IOptions<DriverServiceOptions>>()
                    .Value;

                if (string.IsNullOrWhiteSpace(options.BaseUrl))
                {
                    throw new InvalidOperationException(
                        "DriverService BaseUrl is not configured.");
                }

                client.BaseAddress = new Uri(options.BaseUrl);
            });

        return services;
    }
}