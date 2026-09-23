using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Application.Abstractions
{
    /// <summary>
    /// Handler برای یک نوع payload مشخص.
    /// </summary>
    public interface IScheduledJobHandler<in TPayload>
        where TPayload : IScheduledJobPayload
    {
        Task ExecuteAsync(TPayload payload, CancellationToken ct);
    }
}
