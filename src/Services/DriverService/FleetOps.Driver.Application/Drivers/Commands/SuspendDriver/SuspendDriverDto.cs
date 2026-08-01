using FleetOps.Driver.Domain.Drivers.Enums;

namespace FleetOps.Driver.Application.Drivers.Commands.SuspendDriver
{
    public sealed class SuspendDriverDto
    {
        public DriverSuspensionReason Reason { get; set; }

        public string? Notes { get; set; }
    }
}