using Core.Application.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Scheduler;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.HistoricalData;
using Xunit;

namespace Trader.Tests.Infrastructure.HistoricalData;

public class CollectCandleJobHandlerTests
{
    // ═══════════════════════════════════════════════════════════════
    // Idempotent guard
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExecuteAsync_JobNotFound_NoOp()
    {
        var (sut, deps) = CreateSut();
        deps.Jobs.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((HistoricalDataCollectionJob?)null);

        await sut.ExecuteAsync(new CollectCandlePayload(Guid.NewGuid()), CancellationToken.None);

        deps.Collector.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_JobNotRunning_NoOp()
    {
        var (sut, deps) = CreateSut();
        var job = CreateBackfillJob();   // State = Created
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.ExecuteAsync(new CollectCandlePayload(job.Id), CancellationToken.None);

        deps.Collector.VerifyNoOtherCalls();
    }

    // ═══════════════════════════════════════════════════════════════
    // Mode dispatch
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExecuteAsync_BackfillMode_DispatchesCollectBackfill()
    {
        var (sut, deps) = CreateSut();
        var job = CreateRunningBackfillJob();
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.ExecuteAsync(new CollectCandlePayload(job.Id), CancellationToken.None);

        deps.Collector.Verify(x => x.CollectBackfillAsync(
            job.Id, It.IsAny<CancellationToken>()), Times.Once);
        deps.Collector.Verify(x => x.CollectRecurringAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_RecurringMode_DispatchesCollectRecurring()
    {
        var (sut, deps) = CreateSut();
        var job = CreateRunningRecurringJob();
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.ExecuteAsync(new CollectCandlePayload(job.Id), CancellationToken.None);

        deps.Collector.Verify(x => x.CollectRecurringAsync(
            job.Id, It.IsAny<CancellationToken>()), Times.Once);
        deps.Collector.Verify(x => x.CollectBackfillAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // Error handling
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExecuteAsync_BackfillThrows_MarksFaultedAndRethrows()
    {
        var (sut, deps) = CreateSut();
        var job = CreateRunningBackfillJob();
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        deps.Collector.Setup(x => x.CollectBackfillAsync(
                job.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var act = () => sut.ExecuteAsync(
            new CollectCandlePayload(job.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*boom*");

        job.State.Should().Be(HistoricalJobState.Faulted);
        job.LastError.Should().Be("boom");
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    private static HistoricalDataCollectionJob CreateBackfillJob() =>
        HistoricalDataCollectionJob.CreateBackfill(
            "test", """["TEST"]""", """[1440]""",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 1, 31, 0, 0, 0, DateTimeKind.Utc),
            totalCombinations: 1);

    private static HistoricalDataCollectionJob CreateRunningBackfillJob()
    {
        var job = CreateBackfillJob();
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);
        return job;
    }

    private static HistoricalDataCollectionJob CreateRunningRecurringJob()
    {
        var job = HistoricalDataCollectionJob.CreateRecurring(
            "test", """["TEST"]""", """[1440]""",
            new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            """{"type":3,"interval":5}""",
            totalCombinations: 1);
        job.MarkRunningRecurring(Guid.NewGuid(), DateTime.UtcNow);
        return job;
    }

    private static (CollectCandleJobHandler Sut, TestDeps Deps) CreateSut()
    {
        var uow = new Mock<ITraderUnitOfWork<TraderDbContext>>();
        var jobs = new Mock<IRepository<TraderDbContext, HistoricalDataCollectionJob, Guid>>();
        uow.SetupGet(x => x.HistoricalDataCollectionJobs).Returns(jobs.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var collector = new Mock<IHistoricalDataCollector>();

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ITraderUnitOfWork<TraderDbContext>)))
            .Returns(uow.Object);
        serviceProvider.Setup(x => x.GetService(typeof(IHistoricalDataCollector)))
            .Returns(collector.Object);

        var scope = new Mock<IServiceScope>();
        scope.SetupGet(x => x.ServiceProvider).Returns(serviceProvider.Object);
        scope.Setup(x => x.Dispose());

        var factory = new Mock<IServiceScopeFactory>();
        factory.Setup(x => x.CreateScope()).Returns(scope.Object);

        var sut = new CollectCandleJobHandler(
            factory.Object,
            NullLogger<CollectCandleJobHandler>.Instance);

        return (sut, new TestDeps(uow, jobs, collector));
    }

    private sealed record TestDeps(
        Mock<ITraderUnitOfWork<TraderDbContext>> Uow,
        Mock<IRepository<TraderDbContext, HistoricalDataCollectionJob, Guid>> Jobs,
        Mock<IHistoricalDataCollector> Collector);
}