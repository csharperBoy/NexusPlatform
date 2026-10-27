using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;

namespace Scheduler.Application.Commands.RecurringSchedules;

public record PauseRecurringScheduleCommand(Guid Id) : IRequest<Result<bool>>;

public class PauseRecurringScheduleCommandHandler
    : IRequestHandler<PauseRecurringScheduleCommand, Result<bool>>
{
    private readonly ISchedulerService _scheduler;
    private readonly ILogger<PauseRecurringScheduleCommandHandler> _logger;

    public PauseRecurringScheduleCommandHandler(
        ISchedulerService scheduler,
        ILogger<PauseRecurringScheduleCommandHandler> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        PauseRecurringScheduleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _scheduler.PauseRecurringAsync(
                request.Id, cancellationToken);

            if (!ok)
                return Result<bool>.Fail(
                    $"RecurringSchedule {request.Id} not found or not Active.");

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to pause RecurringSchedule {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}