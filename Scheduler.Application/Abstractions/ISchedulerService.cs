using Scheduler.Application.Models;
using Scheduler.Application.Queries;
using Scheduler.Domain.ValueObjects;
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



        /// <summary>
        /// ثبت یک Recurring Job (تعریف) و زمان‌بندی اولین Occurrence.
        /// Self-Rescheduling: پس از هر اجرا، Occurrence بعدی خودکار Schedule می‌شود.
        /// </summary>
        Task<Guid> ScheduleRecurringAsync<TPayload>(
            string name,
            TPayload payload,
            RecurrencePattern pattern,
            DateTimeOffset? startAt = null,
            DateTimeOffset? endAt = null,
            int? maxOccurrences = null,
            int? maxFailures = 10,
            TimeZoneInfo? timeZone = null,
            string? queue = null,
            CancellationToken ct = default)
            where TPayload : IScheduledJobPayload;

        Task<RecurringScheduleInfo?> GetRecurringAsync(
    Guid recurringScheduleId,
    CancellationToken ct = default);

        Task<IReadOnlyList<RecurringScheduleInfo>> QueryRecurringAsync(
            RecurringSchedulerQuery query,
            CancellationToken ct = default);

        Task<bool> PauseRecurringAsync(
    Guid recurringScheduleId,
    CancellationToken ct = default);

        Task<bool> ResumeRecurringAsync(
            Guid recurringScheduleId,
            CancellationToken ct = default);

        Task<bool> CancelRecurringAsync(
            Guid recurringScheduleId,
            CancellationToken ct = default);
    }
}
