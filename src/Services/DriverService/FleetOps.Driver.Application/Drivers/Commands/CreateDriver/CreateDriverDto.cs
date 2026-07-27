using FleetOps.Driver.Domain.Drivers.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Application.Drivers.Commands.CreateDriver
{
    public class CreateDriverDto
    {
        public string? FullName { get; set; } 
        public string? PhoneNumber { get; set; }
        public string? VehiclePlateNumber { get; set; }
        public VehicleType VehicleType { get; set; }

    }
}
