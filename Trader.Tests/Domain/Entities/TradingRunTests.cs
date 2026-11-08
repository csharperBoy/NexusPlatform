using FluentAssertions;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Xunit;

namespace Trader.Tests.Domain.Entities;

public class TradingRunTests
{
    [Fact]
    public void Create_WithSupportedTargetCurrency_Succeeds()
    {
        var run = CreateRun(targetCurrency: "usd");

        run.TargetCurrency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNullTargetCurrency_Succeeds()
    {
        var run = CreateRun(targetCurrency: null);

        run.TargetCurrency.Should().BeNull();
    }

    [Fact]
    public void Create_WithUnsupportedTargetCurrency_Throws()
    {
        var act = () => CreateRun(targetCurrency: "USDD");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*USDD*not supported*");
    }

    [Fact]
    public void UpdateSettings_WithUnsupportedTargetCurrency_Throws()
    {
        var run = CreateRun(targetCurrency: null);

        var act = () => run.UpdateSettings(
            name: run.Name,
            strategyKey: run.StrategyKey,
            strategyConfigJson: run.StrategyConfigJson,
            timeFrame: run.PrimaryTimeFrame,
            capitalAllocation: run.CapitalAllocation,
            stopLoss: null,
            takeProfit: null,
            defaultOrderMode: run.DefaultOrderMode,
            onMarketClose: run.OnMarketClose,
            onStop: run.OnStop,
            targetCurrency: "INVALID",
            strengthMultiplier: run.StrengthMultiplier,
            minDeltaPercent: run.MinDeltaPercent,
            executionMode: run.ExecutionMode);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*INVALID*not supported*");
    }

    // ────────────────────────────────────────────────────────────
    // Helper
    // ────────────────────────────────────────────────────────────

    private static TradingRun CreateRun(string? targetCurrency)
    {
        return TradingRun.Create(
            name: "test",
            ownerPersonId: Guid.NewGuid(),
            strategyKey: "rsi",
            strategyConfigJson: "{\"period\":14}",
            timeFrame: TimeFrame.D1,
            capitalAllocation: CapitalAllocation.Dynamic(),
            stopLoss: null,
            takeProfit: null,
            defaultOrderMode: OrderMode.Quantity,
            onMarketClose: MarketCloseBehavior.KeepPositions,
            onStop: StopRunBehavior.KeepPositions,
            symbols: new[] { ("TEST", new SymbolAllocation(100m)) }, 
            accounts: new[] { (Guid.NewGuid(), 100m) },
            targetCurrency: targetCurrency);
    }
}