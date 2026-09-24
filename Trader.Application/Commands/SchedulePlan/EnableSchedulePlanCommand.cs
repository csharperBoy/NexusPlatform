using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.SchedulePlan
{
    public record EnableSchedulePlanCommand(Guid Id) : IRequest<Result<bool>>;

    public class EnableSchedulePlanCommandHandler
        : IRequestHandler<EnableSchedulePlanCommand, Result<bool>>
    {
        private readonly ISchedulePlanCommandService _planService;
        private readonly ILogger<EnableSchedulePlanCommandHandler> _logger;

        public EnableSchedulePlanCommandHandler(
            ISchedulePlanCommandService planService,
            ILogger<EnableSchedulePlanCommandHandler> logger)
        {
            _planService = planService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            EnableSchedulePlanCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Enabling SchedulePlan: {Id}", request.Id);

                await _planService.EnableAsync(request.Id);
                await _planService.SaveAsync();

                _logger.LogInformation(
                    "SchedulePlan enabled: {Id}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to enable SchedulePlan: {Id}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}