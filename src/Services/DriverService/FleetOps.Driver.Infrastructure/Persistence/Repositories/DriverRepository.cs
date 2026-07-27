using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using DriverAggregate = FleetOps.Driver.Domain.Drivers.Driver;

namespace FleetOps.Driver.Infrastructure.Persistence.Repositories
{
    public sealed class DriverRepository : IDriverRepository
    {
        private readonly DriverDbContext _context;

        public DriverRepository(DriverDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            DriverAggregate driver,
            CancellationToken cancellationToken = default)
        {
            await _context.Drivers.AddAsync(driver, cancellationToken);
        }

        public async Task<DriverAggregate?> GetByIdAsync( Guid id, CancellationToken cancellationToken = default)
            => await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
               
        
    }
}