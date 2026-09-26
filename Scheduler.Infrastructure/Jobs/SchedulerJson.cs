using System.Text.Json;
using System.Text.Json.Serialization;

namespace Scheduler.Infrastructure.Jobs
{
    internal static class SchedulerJson
    {
        public static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }
}