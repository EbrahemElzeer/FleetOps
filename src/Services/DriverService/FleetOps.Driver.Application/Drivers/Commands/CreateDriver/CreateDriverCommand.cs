using FleetOps.Driver.Domain.Common;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FleetOps.Driver.Application.Drivers.Commands.CreateDriver
{
    public sealed record CreateDriverCommand(CreateDriverDto Dto):IRequest<Result<CreateDriverResponse>>; 
   
}
