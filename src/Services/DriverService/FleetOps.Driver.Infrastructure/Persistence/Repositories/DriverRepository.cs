using FleetOps.Driver.Application.Abstractions;
using FleetOps.Driver.Application.Common.Pagination;
using FleetOps.Driver.Application.Drivers.Queries.GetDrivers;
using FleetOps.Driver.Domain.Drivers.Enums;
using FleetOps.Driver.Infrastructure.Common;
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

        public async Task<PagedResponse<DriverListItemResponse>> GetDriversAsync(
            DriverStatus? status,
            PaginationRequest pagination,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Drivers
                .AsNoTracking()
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status.Value);
            }

            return await query
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DriverListItemResponse(
                    d.Id,
                    d.FullName,
                    d.PhoneNumber,
                    d.VehicleType,
                    d.VehiclePlateNumber,
                    d.Status,
                    d.CreatedAt))
                .ToPagedResponseAsync(pagination, cancellationToken);
        }
    }
}