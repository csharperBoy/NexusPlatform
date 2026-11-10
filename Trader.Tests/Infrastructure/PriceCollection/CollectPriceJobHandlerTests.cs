using Core.Application.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.PriceCollection;
using Trader.Application.Scheduler;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.PriceCollection;
using Xunit;

namespace Trader.Tests.Infrastructure.PriceCollection;

public class CollectPriceJobHandlerTests
{
    // ═══════════════════════════════════════════════════════════════
    // Idempotent guard (D45)
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExecuteAsync_JobNotFound_NoOp()
    {
        var (sut, deps) = CreateSut();
        deps.Jobs.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((PriceCollectionJob?)null);

        await sut.ExecuteAsync(new CollectPricePayload(Guid.NewGuid()), CancellationToken.None);

        deps.Service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_JobStopped_NoOp()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        var job = PriceCollectionJob.CreateBackfill(source.Id);   // State = Created
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.ExecuteAsync(new CollectPricePayload(job.Id), CancellationToken.None);

        deps.Service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ExecuteAsync_JobFaulted_NoOp()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        var job = PriceCollectionJob.CreateBackfill(source.Id);
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);
        job.MarkFaulted("prior failure");
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.ExecuteAsync(new CollectPricePayload(job.Id), CancellationToken.None);

        deps.Service.VerifyNoOtherCalls();
    }

    // ═══════════════════════════════════════════════════════════════
    // Mode dispatch
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExecuteAsync_BackfillMode_CallsCollectBackfill()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        var job = PriceCollectionJob.CreateBackfill(source.Id);
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.ExecuteAsync(new CollectPricePayload(job.Id), CancellationToken.None);

        deps.Service.Verify(x => x.CollectBackfillAsync(job.Id, It.IsAny<CancellationToken>()),
            Times.Once);
        deps.Service.Verify(x => x.CollectOnceAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_RecurringMode_CallsCollectOnce()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        var job = PriceCollectionJob.CreateRecurring(source.Id, """{"type":0,"interval":1}""");
        job.MarkRunningRecurring(Guid.NewGuid(), DateTime.UtcNow);
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        await sut.ExecuteAsync(new CollectPricePayload(job.Id), CancellationToken.None);

        deps.Service.Verify(x => x.CollectOnceAsync(job.Id, It.IsAny<CancellationToken>()),
            Times.Once);
        deps.Service.Verify(x => x.CollectBackfillAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // Error → MarkFaulted → rethrow (D44)
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_MarksFaultedAndRethrows()
    {
        var (sut, deps) = CreateSut();
        var source = PriceSource.Create("x", "X", PriceSourceType.Tgju, "{}", "USD", "IRR");
        var job = PriceCollectionJob.CreateBackfill(source.Id);
        job.MarkRunningBackfill(Guid.NewGuid(), DateTime.UtcNow);
        deps.Jobs.Setup(x => x.GetByIdAsync(job.Id)).ReturnsAsync(job);

        // Service throw
        deps.Service.Setup(x => x.CollectBackfillAsync(
                job.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Second GetById (post-throw reload) returns same running job
        deps.Jobs.SetupSequence(x => x.GetByIdAsync(job.Id))
            .ReturnsAsync(job)
            .ReturnsAsync(job);

        // SaveChanges called after MarkFaulted
        deps.Uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var act = () => sut.ExecuteAsync(
            new CollectPricePayload(job.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*boom*");

        job.State.Should().Be(PriceCollectionJobState.Faulted);
        job.LastError.Should().Be("boom");
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    private static (CollectPriceJobHandler Sut, TestDeps Deps) CreateSut()
    {
        var uow = new Mock<ITraderUnitOfWork<TraderDbContext>>();
        var jobs = new Mock<IRepository<TraderDbContext, PriceCollectionJob, Guid>>();
        uow.SetupGet(x => x.PriceCollectionJobs).Returns(jobs.Object);
        uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new Mock<IPriceCollectionService>();

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(ITraderUnitOfWork<TraderDbContext>)))
            .Returns(uow.Object);
        serviceProvider.Setup(x => x.GetService(typeof(IPriceCollectionService)))
            .Returns(service.Object);

        var scope = new Mock<IServiceScope>();
        scope.SetupGet(x => x.ServiceProvider).Returns(serviceProvider.Object);
        scope.Setup(x => x.Dispose());

        var scopeFactory = new Mock<IServiceScopeFactory>();
        scopeFactory.Setup(x => x.CreateScope()).Returns(scope.Object);

        var sut = new CollectPriceJobHandler(
            scopeFactory.Object,
            NullLogger<CollectPriceJobHandler>.Instance);

        return (sut, new TestDeps(uow, jobs, service));
    }

    private sealed record TestDeps(
        Mock<ITraderUnitOfWork<TraderDbContext>> Uow,
        Mock<IRepository<TraderDbContext, PriceCollectionJob, Guid>> Jobs,
        Mock<IPriceCollectionService> Service);
}