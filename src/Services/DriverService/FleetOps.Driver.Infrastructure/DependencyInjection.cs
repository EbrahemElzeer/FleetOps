using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Infrastructure.Persistence;
using FleetOps.Driver.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FleetOps.Driver.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddDbContext<DriverDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DriverDatabase"));
            });

            services.AddScoped<IDriverRepository, DriverRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}