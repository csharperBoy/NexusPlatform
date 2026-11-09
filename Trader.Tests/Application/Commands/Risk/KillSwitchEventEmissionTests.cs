using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.Risk;
using Trader.Application.Commands.Risk;
using Trader.Domain.Enums;
using Xunit;

namespace Trader.Tests.Application.Commands.Risk;

public class KillSwitchEventEmissionTests
{
    [Fact]
    public async Task Activate_Global_EmitsCriticalEvent()
    {
        var (sut, deps) = CreateActivateSut();

        var result = await sut.Handle(
            new ActivateKillSwitchCommand(Scope: 0, Reason: "market crash"),
            CancellationToken.None);

        result.Succeeded.Should().BeTrue();

        deps.EventLogger.Verify(x => x.Log(
            TradingStage.Risk,
            TradingEventType.KillSwitchActivated,
            TradingEventSeverity.Critical,
            It.IsAny<string>(),
            It.IsAny<object?>(),
            It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<Guid?>(),
            It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Guid?>(), It.IsAny<Guid?>()),
            Times.Once);

        deps.EventLogger.Verify(x => x.FlushAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Activate_Account_EmitsEventWithAccountId()
    {
        var (sut, deps) = CreateActivateSut();
        var accountId = Guid.NewGuid();

        await sut.Handle(
            new ActivateKillSwitchCommand(Scope: 1, AccountId: accountId),
            CancellationToken.None);

        deps.StateTracker.Verify(x => x.ActivateKillSwitchAsync(
            accountId, It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        deps.EventLogger.Verify(x => x.Log(
            It.IsAny<TradingStage>(),
            TradingEventType.KillSwitchActivated,
            It.IsAny<TradingEventSeverity>(),
            It.IsAny<string>(),
            It.IsAny<object?>(),
            It.IsAny<Guid?>(), It.IsAny<string?>(),
            accountId,   // ← accountId match
            It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Guid?>(), It.IsAny<Guid?>()),
            Times.Once);
    }

    [Fact]
    public async Task Deactivate_Global_EmitsInfoEvent()
    {
        var (sut, deps) = CreateDeactivateSut();

        await sut.Handle(
            new DeactivateKillSwitchCommand(Scope: 0),
            CancellationToken.None);

        deps.EventLogger.Verify(x => x.Log(
            TradingStage.Risk,
            TradingEventType.KillSwitchDeactivated,
            TradingEventSeverity.Info,
            It.IsAny<string>(),
            It.IsAny<object?>(),
            It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<Guid?>(),
            It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<Guid?>(),
            It.IsAny<Guid?>(), It.IsAny<Guid?>()),
            Times.Once);

        deps.EventLogger.Verify(x => x.FlushAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Activate_InvalidScope_NoEventEmitted()
    {
        var (sut, deps) = CreateActivateSut();

        var result = await sut.Handle(
            new ActivateKillSwitchCommand(Scope: 99),
            CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("Invalid scope");
        deps.EventLogger.VerifyNoOtherCalls();
        deps.StateTracker.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Activate_AccountScopeWithoutId_ReturnsFail()
    {
        var (sut, deps) = CreateActivateSut();

        var result = await sut.Handle(
            new ActivateKillSwitchCommand(Scope: 1, AccountId: null),
            CancellationToken.None);

        result.Succeeded.Should().BeFalse();
        result.Error.Should().Contain("AccountId is required");
        deps.EventLogger.VerifyNoOtherCalls();
    }

    // ─── Helpers ───

    private static (ActivateKillSwitchCommandHandler Sut, TestDeps Deps)
        CreateActivateSut()
    {
        var deps = CreateDeps();
        var sut = new ActivateKillSwitchCommandHandler(
            deps.StateTracker.Object,
            deps.EventLogger.Object,
            NullLogger<ActivateKillSwitchCommandHandler>.Instance);
        return (sut, deps);
    }

    private static (DeactivateKillSwitchCommandHandler Sut, TestDeps Deps)
        CreateDeactivateSut()
    {
        var deps = CreateDeps();
        var sut = new DeactivateKillSwitchCommandHandler(
            deps.StateTracker.Object,
            deps.EventLogger.Object,
            NullLogger<DeactivateKillSwitchCommandHandler>.Instance);
        return (sut, deps);
    }

    private static TestDeps CreateDeps()
    {
        var stateTracker = new Mock<IRiskStateTracker>();
        stateTracker.Setup(x => x.ActivateKillSwitchAsync(
                It.IsAny<Guid?>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        stateTracker.Setup(x => x.DeactivateKillSwitchAsync(
                It.IsAny<Guid?>(), It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var eventLogger = new Mock<ITradingEventLogger>();
        eventLogger.Setup(x => x.FlushAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new TestDeps(stateTracker, eventLogger);
    }

    private sealed record TestDeps(
        Mock<IRiskStateTracker> StateTracker,
        Mock<ITradingEventLogger> EventLogger);
}