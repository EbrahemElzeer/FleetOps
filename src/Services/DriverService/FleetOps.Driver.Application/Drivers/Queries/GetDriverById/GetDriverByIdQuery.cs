using FleetOps.Driver.Domain.Common;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Queries.GetDriverById
{
    public sealed record GetDriverByIdQuery(Guid DriverId): IRequest<Result<DriverDetailsResponse>>;
        
}