namespace Scheduler.Application.Models;

public class CreateRecurringSchedulerRequest
{
    public string Name { get; set; } = default!;
    public string JobType { get; set; } = default!;
    public string PayloadJson { get; set; } = default!;
    public string PatternJson { get; set; } = default!;
    public DateTimeOffset? StartAt { get; set; }
    public DateTimeOffset? EndAt { get; set; }
    public int? MaxOccurrences { get; set; }
    public int? MaxFailures { get; set; }
    public string? TimeZoneId { get; set; }
}