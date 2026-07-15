using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Commands.CreateOrder
{
    public sealed record CreateOrderLocationDto(
     string Country,
     string Governorate,
     string City,
     string Area,
     string Street,
     string? BuildingNumber,
     string? Landmark,
     decimal Latitude,
     decimal Longitude
 );
}
