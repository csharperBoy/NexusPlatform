using Core.Domain.Specifications;
using Trader.Domain.Entities;
using Trader.Domain.Enums;

namespace Trader.Application.Specifications.HistoricalData;

public sealed class PendingHistoricalJobsSpec
    : BaseSpecification<HistoricalDataCollectionJob>
{
    public PendingHistoricalJobsSpec()
        : base(j => j.Status == CollectionJobStatus.Pending)
    {
        ApplyOrderBy(j => j.CreatedAt);
    }
}