using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Models;

namespace Scheduler.Application.Queries.RecurringSchedules;

public record QueryRecurringSchedulesQuery(RecurringSchedulerQuery Filter)
    : IRequest<Result<IReadOnlyList<RecurringScheduleInfo>>>;

public class QueryRecurringSchedulesQueryHandler
    : IRequestHandler<QueryRecurringSchedulesQuery, Result<IReadOnlyList<RecurringScheduleInfo>>>
{
    private readonly ISchedulerService _scheduler;
    private readonly ILogger<QueryRecurringSchedulesQueryHandler> _logger;

    public QueryRecurringSchedulesQueryHandler(
        ISchedulerService scheduler,
        ILogger<QueryRecurringSchedulesQueryHandler> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<RecurringScheduleInfo>>> Handle(
        QueryRecurringSchedulesQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _scheduler.QueryRecurringAsync(
                request.Filter, cancellationToken);
            return Result<IReadOnlyList<RecurringScheduleInfo>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query RecurringSchedules");
            return Result<IReadOnlyList<RecurringScheduleInfo>>.Fail(ex.Message);
        }
    }
}