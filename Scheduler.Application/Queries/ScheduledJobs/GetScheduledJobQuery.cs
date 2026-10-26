using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Models;

namespace Scheduler.Application.Queries.ScheduledJobs;

public record GetScheduledJobQuery(Guid Id) : IRequest<Result<ScheduledJobInfo>>;

public class GetScheduledJobQueryHandler
    : IRequestHandler<GetScheduledJobQuery, Result<ScheduledJobInfo>>
{
    private readonly ISchedulerService _scheduler;
    private readonly ILogger<GetScheduledJobQueryHandler> _logger;

    public GetScheduledJobQueryHandler(
        ISchedulerService scheduler,
        ILogger<GetScheduledJobQueryHandler> logger)
    {
        _scheduler = scheduler;
        _logger = logger;
    }

    public async Task<Result<ScheduledJobInfo>> Handle(
        GetScheduledJobQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var job = await _scheduler.GetAsync(request.Id, cancellationToken);
            if (job is null)
                return Result<ScheduledJobInfo>.Fail(
                    $"ScheduledJob {request.Id} not found.");

            return Result<ScheduledJobInfo>.Ok(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get ScheduledJob {Id}", request.Id);
            return Result<ScheduledJobInfo>.Fail(ex.Message);
        }
    }
}