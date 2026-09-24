using Core.Application.Results;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Commands.SchedulePlan
{
    public record CreateSchedulePlanCommand(
        string Name,
        string Date,
        bool Enabled,
        string AutoLoginAt,
        string AutoRefreshAt,
        List<ScheduledOrderItemDto> Orders
    ) : IRequest<Result<Guid>>;

    public class CreateSchedulePlanCommandHandler
        : IRequestHandler<CreateSchedulePlanCommand, Result<Guid>>
    {
        private readonly ISchedulePlanCommandService _planService;
        private readonly ILogger<CreateSchedulePlanCommandHandler> _logger;

        public CreateSchedulePlanCommandHandler(
            ISchedulePlanCommandService planService,
            ILogger<CreateSchedulePlanCommandHandler> logger)
        {
            _planService = planService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(
            CreateSchedulePlanCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating SchedulePlan: {Name} date={Date}",
                    request.Name, request.Date);

                var id = await _planService.CreateSchedulePlanAsync(
                    request.Name,
                    request.Date,
                    request.Enabled,
                    request.AutoLoginAt,
                    request.AutoRefreshAt,
                    request.Orders ?? new());

                await _planService.SaveAsync();

                _logger.LogInformation(
                    "SchedulePlan created: {Id} ({Name})", id, request.Name);

                return Result<Guid>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create SchedulePlan: {Name}", request.Name);
                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}