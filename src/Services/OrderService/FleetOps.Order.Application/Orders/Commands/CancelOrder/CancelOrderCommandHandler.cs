using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.CancelOrder
{
    public sealed class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;

        public CancelOrderCommandHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
        }

        public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderId == Guid.Empty) return OrderErrors.OrderIdRequired;

            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null) return OrderErrors.NotFound(request.OrderId);


            var result=  order.Cancel(request.Reason);
            if (result.IsFailure) return result;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
}
