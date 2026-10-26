using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;

namespace Scheduler.Application.Commands.ScheduledJobs;

public record CancelScheduledJobCommand(Guid Id) : IRequest<Result<bool>>;

public class CancelScheduledJobCommandHandler
    : IRequestHandler<CancelScheduledJobCommand, Result<bool>>
{
    private readonly ISchedulerService _scheduler;
    private readonly ILogger<CancelScheduledJobCommandHandler> _logger;

    public CancelScheduledJobCommandHandler(
        ISchedulerService scheduler,
        ILogger<CancelScheduledJobCommandHandler> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CancelScheduledJobCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var cancelled = await _scheduler.CancelAsync(request.Id, cancellationToken);
            if (!cancelled)
                return Result<bool>.Fail(
                    $"ScheduledJob {request.Id} not found or not cancellable.");

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel ScheduledJob {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}