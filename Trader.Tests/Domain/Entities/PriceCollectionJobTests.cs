using FluentAssertions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Xunit;

namespace Trader.Tests.Domain.Entities;

public class PriceCollectionJobTests
{
    [Fact]
    public void CreateRecurring_RequiresPattern()
    {
        var act = () => PriceCollectionJob.CreateRecurring(Guid.NewGuid(), "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateBackfill_StartsInCreatedState()
    {
        var j = PriceCollectionJob.CreateBackfill(Guid.NewGuid());
        j.State.Should().Be(PriceCollectionJobState.Created);
        j.Mode.Should().Be(PriceCollectionMode.Backfill);
    }

    [Fact]
    public void MarkRunningRecurring_SetsStateAndId()
    {
        var j = PriceCollectionJob.CreateRecurring(Guid.NewGuid(), "{}");
        var schedulerId = Guid.NewGuid();

        j.MarkRunningRecurring(schedulerId, DateTime.UtcNow);

        j.State.Should().Be(PriceCollectionJobState.Running);
        j.SchedulerRecurringId.Should().Be(schedulerId);
    }

    [Fact]
    public void MarkRunningRecurring_IsIdempotentForSameId()
    {
        var j = PriceCollectionJob.CreateRecurring(Guid.NewGuid(), "{}");
        var id = Guid.NewGuid();
        var firstStart = DateTime.UtcNow;

        j.MarkRunningRecurring(id, firstStart);
        j.MarkRunningRecurring(id, firstStart.AddMinutes(1));

        j.StartedAt.Should().Be(firstStart);
    }

    [Fact]
    public void MarkRunningBackfill_RequiresBackfillMode()
    {
        var j = PriceCollectionJob.CreateRecurring(Guid.NewGuid(), "{}");
        var act = () => j.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkCompleted_OnlyBackfill()
    {
        var j = PriceCollectionJob.CreateRecurring(Guid.NewGuid(), "{}");
        var act = () => j.MarkCompleted(DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecordRun_AccumulatesTotal()
    {
        var j = PriceCollectionJob.CreateBackfill(Guid.NewGuid());
        j.RecordRun(10, 2, DateTime.UtcNow);
        j.RecordRun(5, 1, DateTime.UtcNow);

        j.TotalInserted.Should().Be(15);
        j.LastRunInserted.Should().Be(5);
        j.LastRunSkipped.Should().Be(1);
    }

    [Fact]
    public void UpdateBackfillProgress_ClampsTo0_100()
    {
        var j = PriceCollectionJob.CreateBackfill(Guid.NewGuid());
        j.UpdateBackfillProgress(150m);
        j.BackfillProgressPercent.Should().Be(100m);

        j.UpdateBackfillProgress(-5m);
        j.BackfillProgressPercent.Should().Be(0m);
    }
}