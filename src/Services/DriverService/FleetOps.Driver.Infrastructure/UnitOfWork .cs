using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Infrastructure.Persistence;

namespace FleetOps.Driver.Infrastructure
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly DriverDbContext _context;

        public UnitOfWork(DriverDbContext context)
        {
            _context = context;
        }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}