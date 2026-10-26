using Scheduler.Domain.Enums;
namespace Scheduler.Application.Models
{

    public record ScheduledJobInfo(
        Guid id,
        string JobType,
        ScheduledJobState State,
        DateTimeOffset FireAt,
        DateTimeOffset? StartedAt,
        DateTimeOffset? CompletedAt,
        string? Error);
}
