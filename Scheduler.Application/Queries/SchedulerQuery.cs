using Scheduler.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Application.Queries
{

    public record SchedulerQuery(
        string? JobType = null,
        Guid? OwnerId = null,
        ScheduledJobState? State = null,
        DateTimeOffset? FireAtFrom = null,
        DateTimeOffset? FireAtTo = null,
        int Skip = 0,
        int Take = 100);
}
