using Core.Application.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.HistoricalData;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.HistoricalData;
using Xunit;

namespace Trader.Tests.Infrastructure.HistoricalData;

public class HistoricalDataCollectorTests
{
    // ═══════════════════════════════════════════════════════════════
    // Skip guard — State != Running
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task CollectBackfillAsync_StateNotRunning_Skips()
    {
        var (sut, deps) = CreateSut();
        var job = CreateBackfillJob();   // State = Created
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.CollectBackfillAsync(job.Id);

        deps.Provider.Verify(x => x.GetCandlesAsync(
            It.IsAny<string>(), It.IsAny<TimeFrame>(),
            It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<DateTime?>()),
            Times.Never);
    }

    [Fact]
    public async Task CollectRecurringAsync_StateNotRunning_Skips()
    {
        var (sut, deps) = CreateSut();
        var job = CreateRecurringJob();   // State = Created
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.CollectRecurringAsync(job.Id);

        deps.Provider.Verify(x => x.GetCandlesAsync(
            It.IsAny<string>(), It.IsAny<TimeFrame>(),
            It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<DateTime?>()),
            Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // Mode mismatch
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task CollectBackfillAsync_OnRecurringMode_Throws()
    {
        var (sut, deps) = CreateSut();
        var job = CreateRecurringJob();
        job.MarkRunningRecurring(Guid.NewGuid(), DateTime.UtcNow);
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        var act = () => sut.CollectBackfillAsync(job.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*requires Mode=Backfill*");
    }

    [Fact]
    public async Task CollectRecurringAsync_OnBackfillMode_Throws()
    {
        var (sut, deps) = CreateSut();
        var job = CreateBackfillJob();
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        var act = () => sut.CollectRecurringAsync(job.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*requires Mode=Recurring*");
    }

    // ═══════════════════════════════════════════════════════════════
    // Happy path — Backfill
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task CollectBackfillAsync_WithCandles_InsertsAndMarksCompleted()
    {
        var (sut, deps) = CreateSut();
        var job = CreateRunningBackfillJob();
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        var candleTime = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
        deps.Provider.SetupSequence(x => x.GetCandlesAsync(
                "TEST", TimeFrame.D1, It.IsAny<int>(),
                It.IsAny<CancellationToken>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new[]
            {
                new Candle(candleTime, 100m, 110m, 90m, 105m, 1000L),
            })
            .ReturnsAsync(Array.Empty<Candle>());

        deps.Persister.Setup(x => x.PersistAsync(
                "TEST", (int)TimeFrame.D1,
                It.IsAny<IReadOnlyList<Candle>>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await sut.CollectBackfillAsync(job.Id);

        job.State.Should().Be(HistoricalJobState.Completed);
        job.TotalCandlesCollected.Should().Be(1);
        deps.Persister.Verify(x => x.PersistAsync(
            "TEST", (int)TimeFrame.D1,
            It.Is<IReadOnlyList<Candle>>(c => c.Count == 1),
            It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
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

    private static (HistoricalDataCollector Sut, TestDeps Deps) CreateSut()
    {
        var jobs = new Mock<IRepository<TraderDbContext, HistoricalDataCollectionJob, Guid>>();
        jobs.Setup(x => x.UpdateAsync(It.IsAny<HistoricalDataCollectionJob>()))
            .Returns(Task.CompletedTask);

        var uow = new Mock<ITraderUnitOfWork<TraderDbContext>>();
        uow.SetupGet(x => x.HistoricalDataCollectionJobs).Returns(jobs.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var provider = new Mock<IMarketDataProvider>();
        provider.SetupGet(x => x.ProviderName).Returns("Test");

        var providerResolver = new Mock<IMarketDataProviderResolver>();
        providerResolver.Setup(x => x.Resolve(MarketType.IranStock)).Returns(provider.Object);

        var persister = new Mock<ICandlePersistenceService>();

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ITraderUnitOfWork<TraderDbContext>)))
            .Returns(uow.Object);
        serviceProvider.Setup(x => x.GetService(typeof(IMarketDataProviderResolver)))
            .Returns(providerResolver.Object);
        serviceProvider.Setup(x => x.GetService(typeof(ICandlePersistenceService)))
            .Returns(persister.Object);

        var scope = new Mock<IServiceScope>();
        scope.SetupGet(x => x.ServiceProvider).Returns(serviceProvider.Object);
        scope.Setup(x => x.Dispose());

        var factory = new Mock<IServiceScopeFactory>();
        factory.Setup(x => x.CreateScope()).Returns(scope.Object);

        var opts = Options.Create(new HistoricalDataCollectionOptions
        {
            RateLimitMs = 0,   // no delay in tests
        });

        var sut = new HistoricalDataCollector(
            factory.Object, opts,
            NullLogger<HistoricalDataCollector>.Instance);

        return (sut, new TestDeps(uow, jobs, provider, persister));
    }

    private sealed record TestDeps(
        Mock<ITraderUnitOfWork<TraderDbContext>> Uow,
        Mock<IRepository<TraderDbContext, HistoricalDataCollectionJob, Guid>> Jobs,
        Mock<IMarketDataProvider> Provider,
        Mock<ICandlePersistenceService> Persister);
}