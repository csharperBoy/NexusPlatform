using Scheduler.Application.Models;
using Scheduler.Application.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Application.Abstractions
{
    public interface ISchedulerService
    {
        /// <summary>
        /// زمان‌بندی یک job جدید.
        /// </summary>
        /// <returns>شناسه‌ی job در سیستم scheduler</returns>
        Task<Guid> ScheduleAsync<TPayload>(
            TPayload payload,
            DateTimeOffset fireAt,
            string? queue = null,
            TimeSpan? preFireBuffer = null,
            CancellationToken ct = default)
            where TPayload : IScheduledJobPayload;

        /// <summary>لغو یک job زمان‌بندی‌شده (اگه هنوز اجرا نشده)</summary>
        Task<bool> CancelAsync(Guid jobId, CancellationToken ct = default);

        /// <summary>دریافت اطلاعات یک job</summary>
        Task<ScheduledJobInfo?> GetAsync(Guid jobId, CancellationToken ct = default);

        /// <summary>جستجوی jobها</summary>
        Task<IReadOnlyList<ScheduledJobInfo>> QueryAsync(
            SchedulerQuery query,
            CancellationToken ct = default);
    }
}
