using FleetOps.Order.Application.Abstractions;
using FleetOps.Order.Domain.Common;
using FleetOps.Order.Domain.Orders;
using MediatR;

namespace FleetOps.Order.Application.Orders.Commands.StartReturnToSender
{
    internal class StartReturnToSenderCommandHandler : IRequestHandler<StartReturnToSenderCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderRepository _orderRepository;
        public StartReturnToSenderCommandHandler(IUnitOfWork unitOfWork, IOrderRepository orderRepository)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
        }

        public async Task<Result> Handle(StartReturnToSenderCommand request, CancellationToken cancellationToken)
        {
            if (request.OrderId == Guid.Empty) return OrderErrors.OrderIdRequired;
          

            var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
            if (order is null) return OrderErrors.NotFound(request.OrderId);

            var result = order.StartReturnToSender();
            if (result.IsFailure) return result;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success;
        }
    }
}