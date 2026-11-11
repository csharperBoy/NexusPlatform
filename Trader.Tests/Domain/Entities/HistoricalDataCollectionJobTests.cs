using FluentAssertions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Xunit;

namespace Trader.Tests.Domain.Entities;

public class HistoricalDataCollectionJobTests
{
    // ═══════════════════════════════════════════════════════════════
    // Factory — CreateBackfill
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void CreateBackfill_ValidArgs_CreatesInCreatedState()
    {
        var job = HistoricalDataCollectionJob.CreateBackfill(
            name: "test",
            isinsJson: """["TEST"]""",
            timeFramesJson: """[1440]""",
            dateFrom: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            dateTo: new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc),
            totalCombinations: 1);

        job.Mode.Should().Be(HistoricalCollectionMode.Backfill);
        job.State.Should().Be(HistoricalJobState.Created);
        job.DateTo.Should().NotBeNull();
        job.RecurrencePatternJson.Should().BeNull();
        job.TotalCombinations.Should().Be(1);
    }

    [Fact]
    public void CreateBackfill_DateToBeforeDateFrom_Throws()
    {
        var act = () => HistoricalDataCollectionJob.CreateBackfill(
            "test", "[]", "[]",
            dateFrom: DateTime.UtcNow,
            dateTo: DateTime.UtcNow.AddDays(-1),
            totalCombinations: 1);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*DateTo must be after DateFrom*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateBackfill_EmptyName_Throws(string name)
    {
        var act = () => HistoricalDataCollectionJob.CreateBackfill(
            name, "[]", "[]",
            DateTime.UtcNow.AddDays(-1), DateTime.UtcNow, 1);

        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // Factory — CreateRecurring
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void CreateRecurring_ValidArgs_CreatesInCreatedState()
    {
        var job = HistoricalDataCollectionJob.CreateRecurring(
            name: "test",
            isinsJson: """["TEST"]""",
            timeFramesJson: """[1440]""",
            dateFrom: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            recurrencePatternJson: """{"type":3,"interval":5}""",
            totalCombinations: 1);

        job.Mode.Should().Be(HistoricalCollectionMode.Recurring);
        job.State.Should().Be(HistoricalJobState.Created);
        job.DateTo.Should().BeNull();
        job.RecurrencePatternJson.Should().NotBeNull();
    }

    [Fact]
    public void CreateRecurring_MissingPattern_Throws()
    {
        var act = () => HistoricalDataCollectionJob.CreateRecurring(
            "test", "[]", "[]", DateTime.UtcNow, "", 1);

        act.Should().Throw<ArgumentException>();
    }

    // ═══════════════════════════════════════════════════════════════
    // State transitions — Backfill
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void MarkRunningBackfill_SetsRunningAndSchedulerId()
    {
        var job = CreateBackfill();
        var schedulerId = Guid.NewGuid();

        job.MarkRunningBackfill(schedulerId, DateTime.UtcNow);

        job.State.Should().Be(HistoricalJobState.Running);
        job.SchedulerJobId.Should().Be(schedulerId);
        job.BackfillProgressPercent.Should().Be(0m);
    }

    [Fact]
    public void MarkRunningBackfill_IsIdempotentForSameId()
    {
        var job = CreateBackfill();
        var schedulerId = Guid.NewGuid();
        var firstStart = DateTime.UtcNow;

        job.MarkRunningBackfill(schedulerId, firstStart);
        job.MarkRunningBackfill(schedulerId, firstStart.AddMinutes(5));

        job.StartedAt.Should().Be(firstStart);
    }

    [Fact]
    public void MarkRunningBackfill_OnRecurringMode_Throws()
    {
        var job = CreateRecurring();
        var act = () => job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Backfill*");
    }

    [Fact]
    public void RecordBackfillProgress_UpdatesPercent()
    {
        var job = CreateBackfill(totalCombinations: 10);
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);

        job.RecordBackfillProgress("TEST", 1440, completedCombinations: 5, totalCandles: 100);

        job.CompletedCombinations.Should().Be(5);
        job.TotalCandlesCollected.Should().Be(100);
        job.BackfillProgressPercent.Should().Be(50m);
        job.CurrentIsin.Should().Be("TEST");
        job.CurrentTimeFrame.Should().Be(1440);
    }

    [Fact]
    public void RecordBackfillProgress_OnRecurringMode_Throws()
    {
        var job = CreateRecurring();
        job.MarkRunningRecurring(Guid.NewGuid(), DateTime.UtcNow);

        var act = () => job.RecordBackfillProgress("TEST", 1440, 1, 10);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Backfill*");
    }

    [Fact]
    public void MarkCompleted_OnlyBackfill()
    {
        var job = CreateRecurring();
        job.MarkRunningRecurring(Guid.NewGuid(), DateTime.UtcNow);

        var act = () => job.MarkCompleted(DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Backfill*");
    }

    [Fact]
    public void MarkCompleted_FromRunning_SetsTerminalState()
    {
        var job = CreateBackfill();
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);

        job.MarkCompleted(DateTime.UtcNow);

        job.State.Should().Be(HistoricalJobState.Completed);
        job.CompletedAt.Should().NotBeNull();
        job.SchedulerJobId.Should().BeNull();
        job.BackfillProgressPercent.Should().Be(100m);
    }

    // ═══════════════════════════════════════════════════════════════
    // State transitions — Recurring
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void MarkRunningRecurring_SetsRunningAndSchedulerRecurringId()
    {
        var job = CreateRecurring();
        var recurringId = Guid.NewGuid();

        job.MarkRunningRecurring(recurringId, DateTime.UtcNow);

        job.State.Should().Be(HistoricalJobState.Running);
        job.SchedulerRecurringId.Should().Be(recurringId);
    }

    [Fact]
    public void MarkRunningRecurring_IsIdempotentForSameId()
    {
        var job = CreateRecurring();
        var recurringId = Guid.NewGuid();
        var firstStart = DateTime.UtcNow;

        job.MarkRunningRecurring(recurringId, firstStart);
        job.MarkRunningRecurring(recurringId, firstStart.AddMinutes(5));

        job.StartedAt.Should().Be(firstStart);
    }

    [Fact]
    public void MarkRunningRecurring_OnBackfillMode_Throws()
    {
        var job = CreateBackfill();
        var act = () => job.MarkRunningRecurring(Guid.NewGuid(), DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Recurring*");
    }

    [Fact]
    public void RecordRecurringRun_AccumulatesTotal()
    {
        var job = CreateRecurring();
        job.MarkRunningRecurring(Guid.NewGuid(), DateTime.UtcNow);

        job.RecordRecurringRun(5, 2, DateTime.UtcNow);
        job.RecordRecurringRun(3, 1, DateTime.UtcNow);

        job.LastRunInserted.Should().Be(3);
        job.LastRunSkipped.Should().Be(1);
        job.TotalCandlesCollected.Should().Be(8);
    }

    [Fact]
    public void RecordRecurringRun_OnBackfillMode_Throws()
    {
        var job = CreateBackfill();
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);

        var act = () => job.RecordRecurringRun(1, 0, DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Recurring*");
    }

    // ═══════════════════════════════════════════════════════════════
    // Stop / Faulted
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public void MarkStopped_FromRunning_SetsStoppedAndClearsSchedulers()
    {
        var job = CreateRecurring();
        job.MarkRunningRecurring(Guid.NewGuid(), DateTime.UtcNow);

        job.MarkStopped(DateTime.UtcNow);

        job.State.Should().Be(HistoricalJobState.Stopped);
        job.SchedulerRecurringId.Should().BeNull();
        job.SchedulerJobId.Should().BeNull();
        job.StoppedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkStopped_FromNonRunning_NoOp()
    {
        var job = CreateBackfill();   // State = Created

        job.MarkStopped(DateTime.UtcNow);

        job.State.Should().Be(HistoricalJobState.Created);
    }

    [Fact]
    public void MarkFaulted_SetsFaultedState()
    {
        var job = CreateBackfill();
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);

        job.MarkFaulted("network error");

        job.State.Should().Be(HistoricalJobState.Faulted);
        job.LastError.Should().Be("network error");
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    private static HistoricalDataCollectionJob CreateBackfill(int totalCombinations = 1)
        => HistoricalDataCollectionJob.CreateBackfill(
            name: "test",
            isinsJson: """["TEST"]""",
            timeFramesJson: """[1440]""",
            dateFrom: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            dateTo: new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc),
            totalCombinations: totalCombinations);

    private static HistoricalDataCollectionJob CreateRecurring()
        => HistoricalDataCollectionJob.CreateRecurring(
            name: "test",
            isinsJson: """["TEST"]""",
            timeFramesJson: """[1440]""",
            dateFrom: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            recurrencePatternJson: """{"type":3,"interval":5}""",
            totalCombinations: 1);
}