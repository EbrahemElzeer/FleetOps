using FleetOps.Driver.Application.Common.Pagination;
using FleetOps.Driver.Application.Drivers.Queries.GetDrivers;
using FleetOps.Driver.Domain.Drivers.Enums;
using DriverAggregate = FleetOps.Driver.Domain.Drivers.Driver;
namespace FleetOps.Driver.Application.Abstractions
{
    public interface IDriverRepository
    {
        Task AddAsync(
            DriverAggregate driver,
            CancellationToken cancellationToken = default);

        Task<DriverAggregate?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);
        Task<PagedResponse<DriverListItemResponse>> GetDriversAsync(
            DriverStatus? status,
            PaginationRequest pagination,
            CancellationToken cancellationToken = default);
    }
}
