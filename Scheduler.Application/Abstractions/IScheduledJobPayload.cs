using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Application.Abstractions
{
    /// <summary>
    /// Marker برای payload یک job.
    /// هر payload باید [ScheduledJob("key")] داشته باشه.
    /// </summary>
    public interface IScheduledJobPayload { }
}
