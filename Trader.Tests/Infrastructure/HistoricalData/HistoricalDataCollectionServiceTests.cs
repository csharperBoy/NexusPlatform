using Core.Application.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Scheduler.Application.Abstractions;
using Scheduler.Domain.ValueObjects;
using Trader.Application.Abstractions;
using Trader.Application.Dtos.HistoricalData;
using Trader.Application.Scheduler;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.HistoricalData;
using Xunit;

namespace Trader.Tests.Infrastructure.HistoricalData;

public class HistoricalDataCollectionServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // CreateBackfillAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateBackfillAsync_Valid_CreatesJob()
    {
        var (sut, deps) = CreateSut();

        var id = await sut.CreateBackfillAsync(NewBackfillModel());

        id.Should().NotBeEmpty();
        deps.Jobs.Verify(x => x.AddAsync(It.Is<HistoricalDataCollectionJob>(
            j => j.Mode == HistoricalCollectionMode.Backfill
              && j.State == HistoricalJobState.Created)), Times.Once);
    }

    [Fact]
    public async Task CreateBackfillAsync_DateToBeforeDateFrom_Throws()
    {
        var (sut, _) = CreateSut();
        var model = NewBackfillModel() with
        {
            DateFrom = DateTime.UtcNow,
            DateTo = DateTime.UtcNow.AddDays(-1),
        };

        var act = () => sut.CreateBackfillAsync(model);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*DateTo must be after DateFrom*");
    }

    [Fact]
    public async Task CreateBackfillAsync_EmptyIsins_Throws()
    {
        var (sut, _) = CreateSut();
        var model = NewBackfillModel() with { Isins = Array.Empty<string>() };

        var act = () => sut.CreateBackfillAsync(model);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*نماد*");
    }

    // ═══════════════════════════════════════════════════════════════
    // CreateRecurringAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateRecurringAsync_Valid_CreatesJob()
    {
        var (sut, deps) = CreateSut();

        var id = await sut.CreateRecurringAsync(NewRecurringModel());

        id.Should().NotBeEmpty();
        deps.Jobs.Verify(x => x.AddAsync(It.Is<HistoricalDataCollectionJob>(
            j => j.Mode == HistoricalCollectionMode.Recurring)), Times.Once);
    }

    [Fact]
    public async Task CreateRecurringAsync_InvalidPattern_Throws()
    {
        var (sut, _) = CreateSut();
        var model = NewRecurringModel() with { RecurrencePatternJson = "not-json" };

        var act = () => sut.CreateRecurringAsync(model);

        await act.Should().ThrowAsync<Exception>();
    }

    // ═══════════════════════════════════════════════════════════════
    // StartJobAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task StartJobAsync_NotFound_Throws()
    {
        var (sut, deps) = CreateSut();
        deps.Jobs.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((HistoricalDataCollectionJob?)null);

        var act = () => sut.StartJobAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task StartJobAsync_AlreadyRunning_Throws()
    {
        var (sut, deps) = CreateSut();
        var job = CreateRunningBackfillJob();
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        var act = () => sut.StartJobAsync(job.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already Running*");
    }

    [Fact]
    public async Task StartJobAsync_Backfill_SchedulesOnceAndMarksRunning()
    {
        var (sut, deps) = CreateSut();
        var job = CreateBackfillJob();
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        var schedulerJobId = Guid.NewGuid();
        deps.Scheduler.Setup(x => x.ScheduleAsync(
                It.IsAny<CollectCandlePayload>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedulerJobId);

        await sut.StartJobAsync(job.Id);

        job.State.Should().Be(HistoricalJobState.Running);
        job.SchedulerJobId.Should().Be(schedulerJobId);
        deps.Jobs.Verify(x => x.UpdateAsync(job), Times.Once);
    }

    [Fact]
    public async Task StartJobAsync_Recurring_SchedulesRecurringAndMarksRunning()
    {
        var (sut, deps) = CreateSut();
        var job = CreateRecurringJob();
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        var recurringId = Guid.NewGuid();
        deps.Scheduler.Setup(x => x.ScheduleRecurringAsync(
                It.IsAny<string>(),
                It.IsAny<CollectCandlePayload>(),
                It.IsAny<RecurrencePattern>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<DateTimeOffset?>(),
                It.IsAny<int?>(),
                It.IsAny<int?>(),
                It.IsAny<TimeZoneInfo?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(recurringId);

        await sut.StartJobAsync(job.Id);

        job.State.Should().Be(HistoricalJobState.Running);
        job.SchedulerRecurringId.Should().Be(recurringId);
    }

    // ═══════════════════════════════════════════════════════════════
    // StopJobAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task StopJobAsync_NotRunning_NoOp()
    {
        var (sut, deps) = CreateSut();
        var job = CreateBackfillJob();   // State = Created
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.StopJobAsync(job.Id);

        deps.Jobs.Verify(x => x.UpdateAsync(It.IsAny<HistoricalDataCollectionJob>()), Times.Never);
        deps.Scheduler.Verify(x => x.CancelAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StopJobAsync_BackfillRunning_CancelsSchedulerAndMarksStopped()
    {
        var (sut, deps) = CreateSut();
        var job = CreateRunningBackfillJob();
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);
        deps.Scheduler.Setup(x => x.CancelAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await sut.StopJobAsync(job.Id);

        deps.Scheduler.Verify(x => x.CancelAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        job.State.Should().Be(HistoricalJobState.Stopped);
    }

    [Fact]
    public async Task StopJobAsync_RecurringRunning_CancelsRecurring()
    {
        var (sut, deps) = CreateSut();
        var job = CreateRecurringJob();
        job.MarkRunningRecurring(Guid.NewGuid(), DateTime.UtcNow);
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);
        deps.Scheduler.Setup(x => x.CancelRecurringAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await sut.StopJobAsync(job.Id);

        deps.Scheduler.Verify(x => x.CancelRecurringAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        job.State.Should().Be(HistoricalJobState.Stopped);
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    private static CreateHistoricalBackfillModel NewBackfillModel() => new(
        Name: "test",
        Isins: new List<string> { "TEST" },
        TimeFrames: new List<int> { 1440 },
        DateFrom: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        DateTo: new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc));

    private static CreateHistoricalRecurringModel NewRecurringModel() => new(
        Name: "test",
        Isins: new List<string> { "TEST" },
        TimeFrames: new List<int> { 1440 },
        DateFrom: new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        RecurrencePatternJson: """{"type":3,"interval":5}""");

    private static HistoricalDataCollectionJob CreateBackfillJob() =>
        HistoricalDataCollectionJob.CreateBackfill(
            "test", """["TEST"]""", """[1440]""",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc),
            totalCombinations: 1);

    private static HistoricalDataCollectionJob CreateRecurringJob() =>
        HistoricalDataCollectionJob.CreateRecurring(
            "test", """["TEST"]""", """[1440]""",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            """{"type":3,"interval":5}""",
            totalCombinations: 1);

    private static HistoricalDataCollectionJob CreateRunningBackfillJob()
    {
        var job = CreateBackfillJob();
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);
        return job;
    }

    private static (HistoricalDataCollectionService Sut, TestDeps Deps) CreateSut()
    {
        var jobs = new Mock<IRepository<TraderDbContext, HistoricalDataCollectionJob, Guid>>();
        jobs.Setup(x => x.AddAsync(It.IsAny<HistoricalDataCollectionJob>()))
            .Returns(Task.CompletedTask);
        jobs.Setup(x => x.UpdateAsync(It.IsAny<HistoricalDataCollectionJob>()))
            .Returns(Task.CompletedTask);

        var uow = new Mock<ITraderUnitOfWork<TraderDbContext>>();
        uow.SetupGet(x => x.HistoricalDataCollectionJobs).Returns(jobs.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var scheduler = new Mock<ISchedulerService>();

        var sut = new HistoricalDataCollectionService(
            uow.Object, scheduler.Object,
            NullLogger<HistoricalDataCollectionService>.Instance);

        return (sut, new TestDeps(uow, jobs, scheduler));
    }

    private sealed record TestDeps(
        Mock<ITraderUnitOfWork<TraderDbContext>> Uow,
        Mock<IRepository<TraderDbContext, HistoricalDataCollectionJob, Guid>> Jobs,
        Mock<ISchedulerService> Scheduler);
}