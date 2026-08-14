using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Application.Sagas.AssignDriver;
using Microsoft.Extensions.DependencyInjection;

namespace FleetOps.Order.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });
        services.AddScoped<IAssignDriverSaga, AssignDriverSaga>();
        return services;
    }
}