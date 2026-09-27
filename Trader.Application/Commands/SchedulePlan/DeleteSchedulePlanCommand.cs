using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;

namespace Trader.Application.Commands.SchedulePlan
{
    public record DeleteSchedulePlanCommand(Guid Id) : IRequest<Result<bool>>;

    public class DeleteSchedulePlanCommandHandler
        : IRequestHandler<DeleteSchedulePlanCommand, Result<bool>>
    {
        private readonly ISchedulePlanCommandService _planService;
        private readonly ILogger<DeleteSchedulePlanCommandHandler> _logger;

        public DeleteSchedulePlanCommandHandler(
            ISchedulePlanCommandService planService,
            ILogger<DeleteSchedulePlanCommandHandler> logger)
        {
            _planService = planService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(
            DeleteSchedulePlanCommand request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Deleting SchedulePlan: {Id}", request.Id);

                await _planService.DeleteSchedulePlanAsync(request.Id);
                await _planService.SaveAsync();

                _logger.LogInformation(
                    "SchedulePlan deleted: {Id}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to delete SchedulePlan: {Id}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}