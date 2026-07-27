using FleetOps.Driver.Domain.Drivers.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Application.Drivers.Commands.CreateDriver
{
    public sealed record CreateDriverResponse(
    Guid Id,
    string FullName,
    string PhoneNumber,
    VehicleType VehicleType,
    string VehiclePlateNumber,
    DriverStatus Status,
    DateTime CreatedAt
);
}
