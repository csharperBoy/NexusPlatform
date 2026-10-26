using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Models;

namespace Scheduler.Application.Queries.ScheduledJobs;

public record QueryScheduledJobsQuery(SchedulerQuery Filter)
    : IRequest<Result<IReadOnlyList<ScheduledJobInfo>>>;

public class QueryScheduledJobsQueryHandler
    : IRequestHandler<QueryScheduledJobsQuery, Result<IReadOnlyList<ScheduledJobInfo>>>
{
    private readonly ISchedulerService _scheduler;
    private readonly ILogger<QueryScheduledJobsQueryHandler> _logger;

    public QueryScheduledJobsQueryHandler(
        ISchedulerService scheduler,
        ILogger<QueryScheduledJobsQueryHandler> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<ScheduledJobInfo>>> Handle(
        QueryScheduledJobsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var list = await _scheduler.QueryAsync(request.Filter, cancellationToken);
            return Result<IReadOnlyList<ScheduledJobInfo>>.Ok(list);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query ScheduledJobs");
            return Result<IReadOnlyList<ScheduledJobInfo>>.Fail(ex.Message);
        }
    }
}