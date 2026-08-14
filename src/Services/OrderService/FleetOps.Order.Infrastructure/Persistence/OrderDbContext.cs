using FleetOps.Order.Domain.Sagas;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Infrastructure.Persistence
{
    public sealed class OrderDbContext : DbContext
    {
        public OrderDbContext(
            DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        public DbSet<Domain.Orders.Order> Orders=> Set<Domain.Orders.Order>();
        public DbSet<AssignDriverSagaState> AssignDriverSagaState => Set<AssignDriverSagaState>();

        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(OrderDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
