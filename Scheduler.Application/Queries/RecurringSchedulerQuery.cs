using Scheduler.Domain.Enums;

namespace Scheduler.Application.Models;

public record RecurringSchedulerQuery(
    string? JobType = null,
    RecurringScheduleState? State = null,
    int Skip = 0,
    int Take = 100);