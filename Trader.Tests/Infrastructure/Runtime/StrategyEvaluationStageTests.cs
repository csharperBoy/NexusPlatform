using Core.Application.Abstractions;
using Core.Application.Context;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Trader.Application.Abstractions;
using Trader.Application.Abstractions.Logging;
using Trader.Application.Abstractions.MarketData;
using Trader.Application.Abstractions.Runtime;
using Trader.Application.Abstractions.Strategies;
using Trader.Application.Abstractions.TradingRuns;
using Trader.Application.Runtime;
using Trader.Domain.Entities;
using Trader.Domain.Enums;
using Trader.Domain.ValueObjects;
using Trader.Infrastructure.Data;
using Trader.Infrastructure.Runtime.Stages;
using Xunit;

namespace Trader.Tests.Infrastructure.Runtime;

public class StrategyEvaluationStageTests
{
    // ─── Mocks ───
    private readonly Mock<IRunExecutionContextCache> _runCache = new();
    private readonly Mock<IMarketSnapshotStore> _snapshotStore = new();
    private readonly Mock<ICandleCache> _candleCache = new();
    private readonly Mock<ISignalChannel> _signalChannel = new();
    private readonly Mock<ITraderUnitOfWork<TraderDbContext>> _uow = new();
    private readonly Mock<IUserContextCache> _userContextCache = new();
    private readonly Mock<ICandleTransformer> _candleTransformer = new();
    private readonly Mock<ITradingEventLogger> _eventLogger = new();
    private readonly Mock<ILogger<StrategyEvaluationStage>> _logger = new();
    private readonly Mock<IRepository<TraderDbContext, StrategyEvaluation, Guid>> _evaluations = new();
    private readonly Mock<IRepository<TraderDbContext, Signal, Guid>> _signals = new();

    private readonly UserDataContext _userContext;
    private readonly StrategyEvaluationStage _sut;

    public StrategyEvaluationStageTests()
    {
        _userContext = UserDataContext.Empty();

        _uow.Setup(x => x.StrategyEvaluations).Returns(_evaluations.Object);
        _uow.Setup(x => x.Signals).Returns(_signals.Object);
        _uow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        _evaluations.Setup(x => x.AddAsync(It.IsAny<StrategyEvaluation>()))
            .Returns(Task.CompletedTask);
        _signals.Setup(x => x.AddAsync(It.IsAny<Signal>()))
            .Returns(Task.CompletedTask);

        _signalChannel.Setup(x => x.WriteAsync(
                It.IsAny<SignalEnvelope>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        _userContextCache.Setup(x => x.GetOrLoadAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_userContext);

        // ─── NEW: StrategyEvaluationService ساخت از همان Mocks ───
        var evaluationService = new Trader.Infrastructure.Runtime.Services.StrategyEvaluationService(
            _snapshotStore.Object,
            _candleCache.Object,
            _candleTransformer.Object,
            _eventLogger.Object,
            new Mock<ILogger<Trader.Infrastructure.Runtime.Services.StrategyEvaluationService>>().Object);

        _sut = new StrategyEvaluationStage(
            _runCache.Object,
            _userContextCache.Object,
            _userContext,
            evaluationService,
            _signalChannel.Object,
            _uow.Object,
            _eventLogger.Object,
            _logger.Object);
    }

    // ═══════════════════════════════════════════════════════════════
    // Test 1 — No snapshot → early return (no I/O beyond cache lookup)
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task RunOnceAsync_NoSnapshot_ReturnsWithoutEvaluation()
    {
        _runCache.Setup(x => x.GetAllActive())
            .Returns(new[] { MakeContext() });
        _snapshotStore.Setup(x => x.GetLatest(It.IsAny<string>()))
            .Returns((MarketSnapshot?)null);

        await _sut.RunOnceAsync();

        _evaluations.Verify(x => x.AddAsync(It.IsAny<StrategyEvaluation>()), Times.Never);
        _signals.Verify(x => x.AddAsync(It.IsAny<Signal>()), Times.Never);
        _candleCache.Verify(x => x.GetOrLoadAsync(
            It.IsAny<string>(), It.IsAny<TimeFrame>(),
            It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // Test 2 — User context load fails → run skipped entirely
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task RunOnceAsync_UserContextLoadFails_SkipsRun()
    {
        _runCache.Setup(x => x.GetAllActive())
            .Returns(new[] { MakeContext() });
        _snapshotStore.Setup(x => x.GetLatest(It.IsAny<string>()))
            .Returns(MakeSnapshot("TEST"));
        _userContextCache.Setup(x => x.GetOrLoadAsync(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        await _sut.RunOnceAsync();

        // اگر User Context fail شود، قبل از هر I/O دیگری Run skip می‌شود
        _candleCache.Verify(x => x.GetOrLoadAsync(
            It.IsAny<string>(), It.IsAny<TimeFrame>(),
            It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _evaluations.Verify(x => x.AddAsync(It.IsAny<StrategyEvaluation>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // Test 3 — Hold signal → evaluation saved, no Signal created
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task RunOnceAsync_HoldResult_SavesEvaluationWithoutSignal()
    {
        var strategy = new FakeStrategy
        {
            RequiredCandleCount = 20,
            Result = StrategyEvaluationResult.Empty(
                new Dictionary<string, object?> { ["rsi"] = 50m })
        };
        var ctx = MakeContext(strategy);

        _runCache.Setup(x => x.GetAllActive()).Returns(new[] { ctx });
        _snapshotStore.Setup(x => x.GetLatest(It.IsAny<string>()))
            .Returns(MakeSnapshot("TEST"));
        _candleCache.Setup(x => x.GetOrLoadAsync(
                "TEST", TimeFrame.D1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCandles(30));

        StrategyEvaluation? captured = null;
        _evaluations
            .Setup(x => x.AddAsync(It.IsAny<StrategyEvaluation>()))
            .Callback<StrategyEvaluation>(e => captured = e)
            .Returns(Task.CompletedTask);

        await _sut.RunOnceAsync();

        captured.Should().NotBeNull();
        captured!.RunId.Should().Be(ctx.RunId);
        captured.Action.Should().Be(SignalAction.Hold);
        captured.FeaturesJson.Should().Contain("rsi");
        _signals.Verify(x => x.AddAsync(It.IsAny<Signal>()), Times.Never);
        _signalChannel.Verify(x => x.WriteAsync(
            It.IsAny<SignalEnvelope>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // Test 4 — Buy signal → full path: eval + signal + envelope + channel
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task RunOnceAsync_BuySignal_FullPath()
    {
        var strategy = new FakeStrategy
        {
            RequiredCandleCount = 20,
            Result = new StrategyEvaluationResult(
                Signals: new[]
                {
                    new StrategySignal(
                        Isin: "TEST",
                        Action: SignalAction.Buy,
                        Strength: new SignalStrength(0.8m),
                        MetadataJson: null)
                },
                Features: new Dictionary<string, object?> { ["rsi"] = 25m })
        };
        var ctx = MakeContext(strategy);

        _runCache.Setup(x => x.GetAllActive()).Returns(new[] { ctx });
        _snapshotStore.Setup(x => x.GetLatest(It.IsAny<string>()))
            .Returns(MakeSnapshot("TEST"));
        _candleCache.Setup(x => x.GetOrLoadAsync(
                "TEST", TimeFrame.D1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCandles(30));

        StrategyEvaluation? capturedEval = null;
        _evaluations
            .Setup(x => x.AddAsync(It.IsAny<StrategyEvaluation>()))
            .Callback<StrategyEvaluation>(e => capturedEval = e)
            .Returns(Task.CompletedTask);

        Signal? capturedSignal = null;
        _signals
            .Setup(x => x.AddAsync(It.IsAny<Signal>()))
            .Callback<Signal>(s => capturedSignal = s)
            .Returns(Task.CompletedTask);

        var capturedEnvelopes = new List<SignalEnvelope>();
        _signalChannel
            .Setup(x => x.WriteAsync(
                It.IsAny<SignalEnvelope>(), It.IsAny<CancellationToken>()))
            .Callback<SignalEnvelope, CancellationToken>(
                (env, _) => capturedEnvelopes.Add(env))
            .Returns(ValueTask.CompletedTask);

        await _sut.RunOnceAsync();

        capturedEval.Should().NotBeNull();
        capturedSignal.Should().NotBeNull();
        capturedEnvelopes.Should().HaveCount(1);
        capturedEnvelopes[0].Signal.Id.Should().Be(capturedSignal!.Id);
        capturedEnvelopes[0].RunId.Should().Be(ctx.RunId);

        // Signal references eval via SourceEvaluationId
        capturedSignal!.SourceEvaluationId.Should().Be(capturedEval!.Id);
        capturedSignal.Action.Should().Be(SignalAction.Buy);
        capturedSignal.Strength.Value.Should().Be(0.8m);

        _uow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    // ═══════════════════════════════════════════════════════════════
    // Test 5 — Currency transform fails → no eval, no signal
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task RunOnceAsync_CurrencyTransformFails_NoEvaluationSaved()
    {
        var strategy = new FakeStrategy
        {
            RequiredCandleCount = 20,
            Result = StrategyEvaluationResult.Empty(
                new Dictionary<string, object?> { ["rsi"] = 50m })
        };
        var ctx = MakeContext(strategy, targetCurrency: "USD");

        _runCache.Setup(x => x.GetAllActive()).Returns(new[] { ctx });
        _snapshotStore.Setup(x => x.GetLatest(It.IsAny<string>()))
            .Returns(MakeSnapshot("TEST"));
        _candleCache.Setup(x => x.GetOrLoadAsync(
                "TEST", TimeFrame.D1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCandles(30));
        _candleTransformer
            .Setup(x => x.TransformAsync(
                "TEST", It.IsAny<IReadOnlyList<Candle>>(),
                "USD", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("No currency rate"));

        await _sut.RunOnceAsync();

        _evaluations.Verify(x => x.AddAsync(It.IsAny<StrategyEvaluation>()), Times.Never);
        _signals.Verify(x => x.AddAsync(It.IsAny<Signal>()), Times.Never);
        _signalChannel.Verify(x => x.WriteAsync(
            It.IsAny<SignalEnvelope>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ═══════════════════════════════════════════════════════════════
    // Test 6 — Currency transform applied → features enriched
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task RunOnceAsync_CurrencyTransformApplied_FeaturesEnriched()
    {
        var strategy = new FakeStrategy
        {
            RequiredCandleCount = 20,
            Result = StrategyEvaluationResult.Empty(
                new Dictionary<string, object?> { ["rsi"] = 50m })
        };
        var ctx = MakeContext(strategy, targetCurrency: "USD");

        _runCache.Setup(x => x.GetAllActive()).Returns(new[] { ctx });
        _snapshotStore.Setup(x => x.GetLatest(It.IsAny<string>()))
            .Returns(MakeSnapshot("TEST"));
        _candleCache.Setup(x => x.GetOrLoadAsync(
                "TEST", TimeFrame.D1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCandles(30));
        _candleTransformer
            .Setup(x => x.TransformAsync(
                "TEST", It.IsAny<IReadOnlyList<Candle>>(),
                "USD", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CandleTransformationResult(
                Candles: MakeCandles(30),
                LastCandleRate: 0.0000004m,
                LastCandleRateEffectiveAt: new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                CandleCountBeforeFirstRate: 0,
                RatePointCount: 1));

        StrategyEvaluation? captured = null;
        _evaluations
            .Setup(x => x.AddAsync(It.IsAny<StrategyEvaluation>()))
            .Callback<StrategyEvaluation>(e => captured = e)
            .Returns(Task.CompletedTask);

        await _sut.RunOnceAsync();

        captured.Should().NotBeNull();
        captured!.FeaturesJson.Should().Contain("\"targetCurrency\":\"USD\"");
        captured.FeaturesJson.Should().Contain("\"priceAtEvalIrr\":");
        captured.FeaturesJson.Should().Contain("\"appliedRate\":");
    }

    // ═══════════════════════════════════════════════════════════════
    // Test 7 — Extra timeframes loaded and passed to strategy
    // ═══════════════════════════════════════════════════════════════

    [Fact]
    public async Task RunOnceAsync_ExtraTimeFramesRequired_PassedToStrategy()
    {
        var strategy = new FakeStrategy
        {
            RequiredCandleCount = 20,
            ExtraCandleRequirements = new[]
            {
                new CandleRequirement(TimeFrame.M15, Count: 5, ClosedOnly: true)
            },
            Result = StrategyEvaluationResult.Empty(
                new Dictionary<string, object?> { ["rsi"] = 50m })
        };
        var ctx = MakeContext(strategy);

        _runCache.Setup(x => x.GetAllActive()).Returns(new[] { ctx });
        _snapshotStore.Setup(x => x.GetLatest(It.IsAny<string>()))
            .Returns(MakeSnapshot("TEST"));
        _candleCache.Setup(x => x.GetOrLoadAsync(
                "TEST", TimeFrame.D1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCandles(30));
        _candleCache.Setup(x => x.GetOrLoadAsync(
                "TEST", TimeFrame.M15, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeCandles(10, TimeSpan.FromMinutes(15)));

        await _sut.RunOnceAsync();

        strategy.LastInput.Should().NotBeNull();
        strategy.LastInput!.ExtraTimeFrames.Should().NotBeNull();
        strategy.LastInput.ExtraTimeFrames!.Should().ContainKey(TimeFrame.M15);
        strategy.LastInput.ExtraTimeFrames[TimeFrame.M15].Should().HaveCount(10);
    }

    // ═══════════════════════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════════════════════

    private static RunRuntimeContext MakeContext(
        IStrategy? strategy = null,
        string? targetCurrency = null)
    {
        strategy ??= new FakeStrategy { RequiredCandleCount = 20 };
        return new RunRuntimeContext(
            RunId: Guid.NewGuid(),
            Name: "test",
            OwnerPersonId: Guid.NewGuid(),
            OwnerUserId: Guid.NewGuid(),
            StrategyKey: "fake",
            Strategy: strategy,
            StrategyConfig: new FakeStrategyConfig(),
            PrimaryTimeFrame: TimeFrame.D1,
            TargetCurrency: targetCurrency,
            StrengthMultiplier: 1.0m,
            MinDeltaPercent: 0.03m,
            ExecutionMode: ExecutionMode.Paper,
            CapitalAllocation: CapitalAllocation.Dynamic(),
            StopLoss: null,
            TakeProfit: null,
            DefaultOrderMode: OrderMode.Quantity,
            OnMarketClose: MarketCloseBehavior.KeepPositions,
            OnStop: StopRunBehavior.KeepPositions,
            Symbols: new[] { new RunSymbolContext("TEST", new SymbolAllocation(100m)) },
AccountAllocations: new[] { new RunAccountAllocation(Guid.NewGuid(), 100m) });
    }

    private static MarketSnapshot MakeSnapshot(string isin) => new(
        Id: Guid.NewGuid(),
        Isin: isin,
        SymbolName: isin,
        StateCode: "OPEN",
        LastTradedPrice: 1_000L,
        ClosingPrice: 1_000L,
        PreviousDayClosingPrice: 990L,
        PriceVar: 0.01,
        FirstTradedPrice: 1_000L,
        HighPrice: 1_010L,
        LowPrice: 990L,
        HighAllowedPrice: 1_050L,
        LowAllowedPrice: 950L,
        PriceTickSize: 1L,
        BestBuyPrice: 999L,
        BestBuyQuantity: 100L,
        BestSellPrice: 1_001L,
        BestSellQuantity: 100L,
        BuyRatio: 1.5m,
        SellRatio: 1.0m,
        MinValidBuyVolume: 1L,
        MaxValidBuyVolume: 100_000L,
        MinValidSellVolume: 1L,
        MaxValidSellVolume: 100_000L,
        TotalTradeValue: 10_000_000L,
        TotalNumberOfTrades: 500L,
        TotalNumberOfSharesTraded: 10_000L,
        CapturedAt: DateTime.UtcNow,
        Source: "test");

    /// <summary>
    /// کندل‌های ثابت در گذشته — تا ClosedCandlesOnly filter رد نکند.
    /// </summary>
    private static IReadOnlyList<Candle> MakeCandles(
        int count, TimeSpan? interval = null)
    {
        var step = interval ?? TimeSpan.FromDays(1);
        var baseTime = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var list = new List<Candle>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(new Candle(
                OpenTime: baseTime + step * i,
                Open: 1_000m,
                High: 1_010m,
                Low: 990m,
                Close: 1_000m,
                Volume: 1_000));
        }
        return list;
    }

    // ─── Fake implementations ───

    private sealed class FakeStrategy : IStrategy
    {
        public string Key => "fake";
        public string DisplayName => "Fake";
        public int RequiredCandleCount { get; set; } = 20;
        public bool RequiresClosedCandlesOnly { get; set; } = true;
        public IReadOnlyList<CandleRequirement> ExtraCandleRequirements { get; set; }
            = Array.Empty<CandleRequirement>();
        public StrategyEvaluationResult Result { get; set; }
            = StrategyEvaluationResult.Empty();
        public StrategyInput? LastInput { get; private set; }

        public StrategyEvaluationResult Evaluate(StrategyInput input)
        {
            LastInput = input;
            return Result;
        }
    }

    private sealed class FakeStrategyConfig : StrategyConfig
    {
        public override string StrategyKey => "fake";
    }
}