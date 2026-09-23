using Scheduler.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
