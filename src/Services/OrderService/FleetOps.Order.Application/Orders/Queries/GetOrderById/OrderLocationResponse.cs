using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Queries.GetOrderById
{
    public sealed record OrderLocationResponse(
     string Country,
     string Governorate,
     string City,
     string Area,
     string Street,
     string? BuildingNumber,
     string? Landmark,
     decimal Latitude,
     decimal Longitude,
     string FormattedAddress
 );
}
