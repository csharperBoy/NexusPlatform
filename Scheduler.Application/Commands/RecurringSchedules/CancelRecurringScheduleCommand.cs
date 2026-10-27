using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;

namespace Scheduler.Application.Commands.RecurringSchedules;

public record CancelRecurringScheduleCommand(Guid Id) : IRequest<Result<bool>>;

public class CancelRecurringScheduleCommandHandler
    : IRequestHandler<CancelRecurringScheduleCommand, Result<bool>>
{
    private readonly ISchedulerService _scheduler;
    private readonly ILogger<CancelRecurringScheduleCommandHandler> _logger;

    public CancelRecurringScheduleCommandHandler(
        ISchedulerService scheduler,
        ILogger<CancelRecurringScheduleCommandHandler> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        CancelRecurringScheduleCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ok = await _scheduler.CancelRecurringAsync(
                request.Id, cancellationToken);

            if (!ok)
                return Result<bool>.Fail(
                    $"RecurringSchedule {request.Id} not found or already terminal.");

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to cancel RecurringSchedule {Id}", request.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}