using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Models;

namespace Scheduler.Application.Queries.RecurringSchedules;

public record GetRecurringScheduleQuery(Guid Id)
    : IRequest<Result<RecurringScheduleInfo>>;

public class GetRecurringScheduleQueryHandler
    : IRequestHandler<GetRecurringScheduleQuery, Result<RecurringScheduleInfo>>
{
    private readonly ISchedulerService _scheduler;
    private readonly ILogger<GetRecurringScheduleQueryHandler> _logger;

    public GetRecurringScheduleQueryHandler(
        ISchedulerService scheduler,
        ILogger<GetRecurringScheduleQueryHandler> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task<Result<RecurringScheduleInfo>> Handle(
        GetRecurringScheduleQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var info = await _scheduler.GetRecurringAsync(request.Id, cancellationToken);
            if (info is null)
                return Result<RecurringScheduleInfo>.Fail(
                    $"RecurringSchedule {request.Id} not found.");

            return Result<RecurringScheduleInfo>.Ok(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to get RecurringSchedule {Id}", request.Id);
            return Result<RecurringScheduleInfo>.Fail(ex.Message);
        }
    }
}