using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders;
using FleetOps.Order.Domain.Orders.Enums;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.AssignDriver
{
    public class  AssignDriverCommandHandler : IRequestHandler<AssignDriverCommand,Result>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IAssignDriverSaga _assignDriverSaga;
        public AssignDriverCommandHandler(IOrderRepository orderRepository, IAssignDriverSaga assignDriverSaga)
        {
            _orderRepository = orderRepository;
            _assignDriverSaga = assignDriverSaga;
        }
        public async Task<Result>  Handle(AssignDriverCommand request, CancellationToken cancellationToken)
        {

            var order= await _orderRepository.GetByIdAsync(request.OrderId,cancellationToken);
            if (order is null) return OrderErrors.NotFound(request.OrderId);

            if (order.Status != OrderStatus.Pending)
                return OrderErrors.CannotAssignDriver(order.Status);
       
            return await _assignDriverSaga.ExecuteAsync(
                order,
                request.DriverId,
                cancellationToken);


        }
    }
}
