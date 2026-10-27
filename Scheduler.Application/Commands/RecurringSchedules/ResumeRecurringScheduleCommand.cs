using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;

namespace Scheduler.Application.Commands.RecurringSchedules;

public record ResumeRecurringScheduleCommand(Guid Id) : IRequest<Result<bool>>;

public class ResumeRecurringScheduleCommandHandler
    : IRequestHandler<ResumeRecurringScheduleCommand, Result<bool>>
{
    private readonly ISchedulerService _scheduler;
    private readonly ILogger<ResumeRecurringScheduleCommandHandler> _logger;

    public ResumeRecurringScheduleCommandHandler(
        ISchedulerService scheduler,
        ILogger<ResumeRecurringScheduleCommandHandler> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        ResumeRecurringScheduleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _scheduler.ResumeRecurringAsync(
                request.Id, cancellationToken);

            if (!ok)
                return Result<bool>.Fail(
                    $"RecurringSchedule {request.Id} not found, not Paused, or bounds exceeded.");

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to resume RecurringSchedule {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}