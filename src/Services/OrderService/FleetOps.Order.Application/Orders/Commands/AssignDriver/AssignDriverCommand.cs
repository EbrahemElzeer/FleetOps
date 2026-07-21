using FleetOps.Order.Application.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Order.Application.Orders.Commands.AssignDriver
{
    public sealed record AssignDriverCommand(Guid OrderId,Guid DriverId):IRequest<Result>;
   
}
