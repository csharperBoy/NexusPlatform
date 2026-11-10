using Core.Application.Abstractions;
using Core.Domain.Specifications;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Scheduler.Application.Abstractions;
using Scheduler.Domain.ValueObjects;
using System.Linq.Expressions;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Application.Dtos.PriceCollection;
using Trader.Application.Scheduler;
using Trader.Application.Specifications.CurrencyRates;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.PriceCollection;
using Xunit;

namespace Trader.Tests.Infrastructure.PriceCollection;

public class PriceCollectionServiceTests
{
    // ═══════════════════════════════════════════════════════════════
    // CreateSourceAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateSourceAsync_NewSource_AddsToRepository()
    {
        var (sut, deps) = CreateSut();

        var id = await sut.CreateSourceAsync(NewSourceModel("tgju-usd"));

        id.Should().NotBeEmpty();
        deps.Sources.Verify(x => x.AddAsync(
            It.Is<PriceSource>(s =>
                s.Code == "tgju-usd" &&
                s.FromAsset == "USD" &&
                s.ToAsset == "IRR" &&
                s.IsActive)),
            Times.Once);
    }

    [Fact]
    public async Task CreateSourceAsync_DuplicateCode_Throws()
    {
        var (sut, deps) = CreateSut();
        deps.SetSourcesResponse(PriceSource.Create(
            "dup", "Existing", PriceSourceType.Tgju,
            "{}", "USD", "IRR"));

        var act = () => sut.CreateSourceAsync(NewSourceModel("dup"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
        deps.Sources.Verify(x => x.AddAsync(It.IsAny<PriceSource>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // UpdateSourceAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task UpdateSourceAsync_NotFound_Throws()
    {
        var (sut, deps) = CreateSut();
        deps.Sources.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PriceSource?)null);

        var act = () => sut.UpdateSourceAsync(Guid.NewGuid(), NewSourceModel("x"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ═══════════════════════════════════════════════════════════════
    // CreateJobAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task CreateJobAsync_SourceNotFound_Throws()
    {
        var (sut, deps) = CreateSut();
        deps.Sources.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PriceSource?)null);

        var act = () => sut.CreateJobAsync(new CreatePriceCollectionJobModel(
            Guid.NewGuid(), (int)PriceCollectionMode.Backfill, null));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task CreateJobAsync_SourceInactive_Throws()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        source.Deactivate();
        deps.Sources.Setup(x => x.GetByIdAsync(source.Id)).ReturnsAsync(source);

        var act = () => sut.CreateJobAsync(new CreatePriceCollectionJobModel(
            source.Id, (int)PriceCollectionMode.Backfill, null));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public async Task CreateJobAsync_RecurringWithoutPattern_Throws()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        deps.Sources.Setup(x => x.GetByIdAsync(source.Id)).ReturnsAsync(source);

        var act = () => sut.CreateJobAsync(new CreatePriceCollectionJobModel(
            source.Id, (int)PriceCollectionMode.Recurring, null));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*RecurrencePatternJson*");
    }

    [Fact]
    public async Task CreateJobAsync_ValidRecurring_AddsRecurringJob()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        deps.Sources.Setup(x => x.GetByIdAsync(source.Id)).ReturnsAsync(source);

        await sut.CreateJobAsync(new CreatePriceCollectionJobModel(
            source.Id, (int)PriceCollectionMode.Recurring, "{}"));

        deps.Jobs.Verify(x => x.AddAsync(It.Is<PriceCollectionJob>(
            j => j.Mode == PriceCollectionMode.Recurring)), Times.Once);
    }

    // ═══════════════════════════════════════════════════════════════
    // StartJobAsync
    // ═══════════════════════════════════════════════════════════════

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
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        var job = PriceCollectionJob.CreateBackfill(source.Id);

        deps.Sources.Setup(x => x.GetByIdAsync(source.Id)).ReturnsAsync(source);
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        var schedulerJobId = Guid.NewGuid();
        deps.Scheduler.Setup(x => x.ScheduleAsync(
                It.IsAny<IScheduledJobPayload>(),
                It.IsAny<DateTimeOffset>(),
                It.IsAny<string?>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedulerJobId);

        await sut.StartJobAsync(job.Id);

        job.State.Should().Be(PriceCollectionJobState.Running);
        job.SchedulerJobId.Should().Be(schedulerJobId);
        deps.Jobs.Verify(x => x.UpdateAsync(job), Times.Once);
    }

    [Fact]
    public async Task StartJobAsync_Recurring_SchedulesRecurringAndMarksRunning()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        var job = PriceCollectionJob.CreateRecurring(source.Id, """{"type":0,"interval":1}""");

        deps.Sources.Setup(x => x.GetByIdAsync(source.Id)).ReturnsAsync(source);
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        var recurringId = Guid.NewGuid();
        deps.Scheduler.Setup(x => x.ScheduleRecurringAsync(
                It.IsAny<string>(),
                It.IsAny<IScheduledJobPayload>(),
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

        job.State.Should().Be(PriceCollectionJobState.Running);
        job.SchedulerRecurringId.Should().Be(recurringId);
    }

    // ═══════════════════════════════════════════════════════════════
    // StopJobAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task StopJobAsync_NotRunning_NoOp()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        var job = PriceCollectionJob.CreateBackfill(source.Id);
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.StopJobAsync(job.Id);

        deps.Jobs.Verify(x => x.UpdateAsync(It.IsAny<PriceCollectionJob>()), Times.Never);
        deps.Scheduler.Verify(x => x.CancelAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task StopJobAsync_RecurringRunning_CancelsScheduler()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        var job = PriceCollectionJob.CreateRecurring(source.Id, """{"type":0,"interval":1}""");
        job.MarkRunningRecurring(Guid.NewGuid(), DateTime.UtcNow);

        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);
        deps.Scheduler.Setup(x => x.CancelRecurringAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await sut.StopJobAsync(job.Id);

        deps.Scheduler.Verify(x => x.CancelRecurringAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        job.State.Should().Be(PriceCollectionJobState.Stopped);
    }

    // ═══════════════════════════════════════════════════════════════
    // CollectBackfillAsync
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task CollectBackfillAsync_JobNotRunning_DoesNothing()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        var job = PriceCollectionJob.CreateBackfill(source.Id);

        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.CollectBackfillAsync(job.Id);

        deps.Providers[0].Verify(x => x.FetchAsync(
            It.IsAny<PriceSource>(), It.IsAny<DateTime?>(),
            It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CollectBackfillAsync_NoPoints_MarksCompleted()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju,
            """{"indicator":"x"}""", "USD", "IRR");
        var job = CreateRunningBackfillJobFor(source);

        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);
        deps.Sources.Setup(x => x.GetByIdAsync(source.Id)).ReturnsAsync(source);

        deps.Providers[0].Setup(x => x.FetchAsync(
                source, It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceFetchResult(Array.Empty<PricePoint>(), 1, true));

        await sut.CollectBackfillAsync(job.Id);

        job.State.Should().Be(PriceCollectionJobState.Completed);
        job.TotalInserted.Should().Be(0);
    }

    [Fact]
    public async Task CollectBackfillAsync_WithPoints_InsertsAndRecords()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju,
            """{"indicator":"x"}""", "USD", "IRR");
        var job = CreateRunningBackfillJobFor(source);

        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);
        deps.Sources.Setup(x => x.GetByIdAsync(source.Id)).ReturnsAsync(source);
        deps.CurrencyRateSpecs.Setup(x => x.ListBySpecAsync(
                It.IsAny<ISpecification<CurrencyRate>>()))
            .ReturnsAsync(Array.Empty<CurrencyRate>());
        deps.CurrencyRateSpecs.Setup(x => x.GetBySpecAsync(
                It.IsAny<ISpecification<CurrencyRate>>()))
            .ReturnsAsync((CurrencyRate?)null);

        var effectiveAt = new DateTime(2026, 11, 1, 20, 30, 0, DateTimeKind.Utc);
        deps.Providers[0].Setup(x => x.FetchAsync(
                source, It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PriceFetchResult(
                new[] { new PricePoint(2_500_000m, effectiveAt, "x") },
                PagesFetched: 1,
                HitBoundary: true));

        await sut.CollectBackfillAsync(job.Id);

        job.State.Should().Be(PriceCollectionJobState.Completed);
        job.TotalInserted.Should().Be(1);

        var stored = await deps.Db.CurrencyRates.ToListAsync();
        stored.Should().HaveCount(1);
        stored[0].Rate.Should().Be(2_500_000m);
        stored[0].EffectiveAt.Should().Be(effectiveAt);
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    private static CreatePriceSourceModel NewSourceModel(string code) => new(
        code, "Test", (int)PriceSourceType.Tgju,
        """{"indicator":"price_dollar_rl"}""", "USD", "IRR");

    private static PriceCollectionJob CreateRunningBackfillJob()
    {
        var job = PriceCollectionJob.CreateBackfill(Guid.NewGuid());
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);
        return job;
    }

    private static PriceCollectionJob CreateRunningBackfillJobFor(PriceSource source)
    {
        var job = PriceCollectionJob.CreateBackfill(source.Id);
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);
        return job;
    }

    private static (PriceCollectionService Sut, TestDeps Deps) CreateSut()
    {
        // TraderDbContext واقعی InMemory — فقط برای _uow.Context.CurrencyRates.AddRangeAsync
        var db = new TraderDbContext(
            new DbContextOptionsBuilder<TraderDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        var sources = new Mock<IRepository<TraderDbContext, PriceSource, Guid>>();
        var jobs = new Mock<IRepository<TraderDbContext, PriceCollectionJob, Guid>>();
        var currencyRateSpecs = new Mock<ISpecificationRepository<CurrencyRate, Guid>>();

        sources.Setup(x => x.AddAsync(It.IsAny<PriceSource>())).Returns(Task.CompletedTask);
        jobs.Setup(x => x.AddAsync(It.IsAny<PriceCollectionJob>())).Returns(Task.CompletedTask);
        jobs.Setup(x => x.UpdateAsync(It.IsAny<PriceCollectionJob>())).Returns(Task.CompletedTask);

        var uow = new Mock<ITraderUnitOfWork<TraderDbContext>>();
        uow.SetupGet(x => x.Context).Returns(db);
        uow.SetupGet(x => x.PriceSources).Returns(sources.Object);
        uow.SetupGet(x => x.PriceCollectionJobs).Returns(jobs.Object);
        uow.SetupGet(x => x.CurrencyRateSpecs).Returns(currencyRateSpecs.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var provider = new Mock<IPriceSourceProvider>();
        provider.SetupGet(x => x.SourceType).Returns(PriceSourceType.Tgju);

        var providerResolver = new Mock<IPriceSourceProviderResolver>();
        providerResolver.Setup(x => x.Resolve(PriceSourceType.Tgju)).Returns(provider.Object);

        var scheduler = new Mock<ISchedulerService>();

        var opts = Options.Create(new PriceCollectionOptions
        {
            DefaultMinRate = 100_000m,
            DefaultMaxRate = 50_000_000m,
            DefaultMaxDailyChangePct = 20m,
        });

        var sut = new PriceCollectionService(
            uow.Object,
            providerResolver.Object,
            scheduler.Object,
            opts,
            NullLogger<PriceCollectionService>.Instance);

        return (sut, new TestDeps(
            sources, jobs, currencyRateSpecs, scheduler,
            providerResolver, new[] { provider }, db));
    }

    private sealed record TestDeps(
        Mock<IRepository<TraderDbContext, PriceSource, Guid>> Sources,
        Mock<IRepository<TraderDbContext, PriceCollectionJob, Guid>> Jobs,
        Mock<ISpecificationRepository<CurrencyRate, Guid>> CurrencyRateSpecs,
        Mock<ISchedulerService> Scheduler,
        Mock<IPriceSourceProviderResolver> ProviderResolver,
        Mock<IPriceSourceProvider>[] Providers,
        TraderDbContext Db)
    {
        /// <summary>
        /// Default response برای GetAllAsync با queryOptions.
        /// نکته: در Service، CreateSourceAsync از GetAllAsync استفاده می‌کند.
        /// </summary>
        public void SetSourcesResponse(params PriceSource[] existing)
        {
            var list = existing.ToList();
            Sources.Setup(x => x.GetAllAsync(
                    It.IsAny<Func<IQueryable<PriceSource>, IOrderedQueryable<PriceSource>>>(),
                    It.IsAny<Expression<Func<PriceSource, object>>[]>()))
                .ReturnsAsync(list);
        }
    }
}