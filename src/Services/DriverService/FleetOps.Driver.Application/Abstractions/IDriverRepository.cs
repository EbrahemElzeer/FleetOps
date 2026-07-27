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
    }
}
