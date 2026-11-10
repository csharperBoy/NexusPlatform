using System.Text.Json.Serialization;
using Scheduler.Application.Abstractions;
using Scheduler.Application.Attributes;

namespace Trader.Application.Scheduler;

[ScheduledJob(Key)]
public sealed record CollectPricePayload(
    [property: JsonPropertyName("jobId")] Guid JobId
) : IScheduledJobPayload
{
    public const string Key = "trader.price-collect";
}