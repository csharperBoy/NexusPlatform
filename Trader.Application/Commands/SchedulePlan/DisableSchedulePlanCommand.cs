using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.SchedulePlan
{
    public record DisableSchedulePlanCommand(Guid Id) : IRequest<Result<bool>>;

    public class DisableSchedulePlanCommandHandler
        : IRequestHandler<DisableSchedulePlanCommand, Result<bool>>
    {
        private readonly ISchedulePlanCommandService _planService;
        private readonly ILogger<DisableSchedulePlanCommandHandler> _logger;

        public DisableSchedulePlanCommandHandler(
            ISchedulePlanCommandService planService,
            ILogger<DisableSchedulePlanCommandHandler> logger)
        {
            _planService = planService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            DisableSchedulePlanCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Disabling SchedulePlan: {Id}", request.Id);

                await _planService.DisableAsync(request.Id);
                await _planService.SaveAsync();

                _logger.LogInformation(
                    "SchedulePlan disabled: {Id}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to disable SchedulePlan: {Id}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}