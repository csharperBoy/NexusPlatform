namespace Scheduler.Application.Abstractions;

public interface IRecurringSchedulePayloadFactory
{
    Type? ResolveType(string jobType);
}