using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using DriverAggregate = FleetOps.Driver.Domain.Drivers.Driver;

namespace FleetOps.Driver.Infrastructure.Persistence
{
    public sealed class DriverDbContext : DbContext
    {
        public DriverDbContext(DbContextOptions<DriverDbContext> options) : base(options)
        {
        }

        public DbSet<DriverAggregate> Drivers => Set<DriverAggregate>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(DriverDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}