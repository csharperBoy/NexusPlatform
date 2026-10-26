using Scheduler.Application.Abstractions;

namespace Scheduler.Infrastructure.Jobs;

public class RecurringSchedulePayloadFactory : IRecurringSchedulePayloadFactory
{
    private readonly JobTypeRegistry _registry;

    public RecurringSchedulePayloadFactory(JobTypeRegistry registry)
    {
        _registry = registry;
    }

    public Type? ResolveType(string jobType)
    {
        if (string.IsNullOrWhiteSpace(jobType))
            return null;
        return _registry.Resolve(jobType);
    }
}