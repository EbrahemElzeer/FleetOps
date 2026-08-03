using FleetOps.Driver.Domain.Common;
using MediatR;

namespace FleetOps.Driver.Application.Drivers.Queries.CheckDriverEligibility
{
    public sealed record CheckDriverEligibilityQuery(Guid DriverId): IRequest<Result<DriverEligibilityResponse>>;
       
}