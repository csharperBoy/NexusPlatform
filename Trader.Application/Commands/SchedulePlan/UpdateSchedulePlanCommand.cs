using Core.Application.Results;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Application.Dtos;

namespace Trader.Application.Commands.SchedulePlan
{
    public record UpdateSchedulePlanCommand(
        Guid Id,
        string Name,
        string Date,
        bool Enabled,
        string AutoLoginAt,
        string AutoRefreshAt,
        List<ScheduledOrderItemDto> Orders
    ) : IRequest<Result<Guid>>;

    public class UpdateSchedulePlanCommandHandler
        : IRequestHandler<UpdateSchedulePlanCommand, Result<Guid>>
    {
        private readonly ISchedulePlanCommandService _planService;
        private readonly ILogger<UpdateSchedulePlanCommandHandler> _logger;

        public UpdateSchedulePlanCommandHandler(
            ISchedulePlanCommandService planService,
            ILogger<UpdateSchedulePlanCommandHandler> logger)
        {
            _planService = planService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(
            UpdateSchedulePlanCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Updating SchedulePlan: {Id}", request.Id);

                var id = await _planService.UpdateSchedulePlanAsync(
                    request.Id,
                    request.Name,
                    request.Date,
                    request.Enabled,
                    request.AutoLoginAt,
                    request.AutoRefreshAt,
                    request.Orders ?? new());

                await _planService.SaveAsync();

                _logger.LogInformation(
                    "SchedulePlan updated: {Id}", id);

                return Result<Guid>.Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update SchedulePlan: {Id}", request.Id);
                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}