using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trader.Application.Abstractions;
using Trader.Domain.Entities;
using Trader.Infrastructure.Data;

namespace Trader.Infrastructure.Data;

public class TraderUnitOfWork : EfUnitOfWork<TraderDbContext>, ITraderUnitOfWork<TraderDbContext>
{
    public TraderUnitOfWork(
        IRepository<TraderDbContext, TraderAccount, Guid> accounts,
        IRepository<TraderDbContext, Instrument, Guid> instruments,
        IRepository<TraderDbContext, InstrumentOrderPreset, Guid> instrumentOrderPresets,
        IRepository<TraderDbContext, TradingRun, Guid> tradingRuns,
        IRepository<TraderDbContext, Signal, Guid> signals,
        IRepository<TraderDbContext, OrderIntent, Guid> orderIntents,
        IRepository<TraderDbContext, BrokerOrder, Guid> brokerOrders,
        IRepository<TraderDbContext, Position, Guid> positions,
        IRepository<TraderDbContext, SchedulePlan, Guid> schedulePlans,
        IRepository<TraderDbContext, ScheduledOrder, Guid> scheduledOrders,
        IRepository<TraderDbContext, ExecutionLog, Guid> executionLogs,
        IRepository<TraderDbContext, TradingSchedule, Guid> tradingSchedules,
        ISpecificationRepository<TradingRun, Guid> tradingRunSpecs,
        ISpecificationRepository<Signal, Guid> signalSpecs,
        ISpecificationRepository<OrderIntent, Guid> orderIntentSpecs,
        ISpecificationRepository<BrokerOrder, Guid> brokerOrderSpecs,
        ISpecificationRepository<Position, Guid> positionSpecs, 
        ISpecificationRepository<Instrument, Guid> instrumentSpecs,
        ISpecificationRepository<InstrumentOrderPreset, Guid> instrumentOrderPresetSpecs,
        ISpecificationRepository<TraderAccount, Guid> accountSpecs,
        ISpecificationRepository<ExecutionLog, Guid> executionLogSpecs,
        ISpecificationRepository<TradingSchedule, Guid> tradingScheduleSpecs,
        TraderDbContext dbContext,
        IOutboxService<TraderDbContext> outboxService,
        IRepository<TraderDbContext, OrderTrade, Guid> orderTrades,
    ISpecificationRepository<OrderTrade, Guid> orderTradeSpecs,
    IRepository<TraderDbContext, StrategyEvaluation, Guid> strategyEvaluations,
ISpecificationRepository<StrategyEvaluation, Guid> strategyEvaluationSpecs,
    IRepository<TraderDbContext, MarketSnapshotRecord, Guid> marketSnapshots,
    ISpecificationRepository<MarketSnapshotRecord, Guid> marketSnapshotSpecs,
    IRepository<TraderDbContext, CandleRecord, Guid> candleRecords,
ISpecificationRepository<CandleRecord, Guid> candleRecordSpecs,
        ILogger<EfUnitOfWork<TraderDbContext>> logger)
        : base(dbContext, outboxService, logger)
    {
        Accounts = accounts;
        Instruments = instruments;
        InstrumentOrderPresets = instrumentOrderPresets;
        TradingRuns = tradingRuns;
        Signals = signals;
        OrderIntents = orderIntents;
        BrokerOrders = brokerOrders;
        Positions = positions;
        SchedulePlans = schedulePlans;
        ScheduledOrders = scheduledOrders;
        ExecutionLogs = executionLogs;

        TradingRunSpecs = tradingRunSpecs;
        SignalSpecs = signalSpecs;
        OrderIntentSpecs = orderIntentSpecs;
        BrokerOrderSpecs = brokerOrderSpecs;
        InstrumentSpecs = instrumentSpecs;
        InstrumentOrderPresetSpecs = instrumentOrderPresetSpecs;
        PositionSpecs = positionSpecs;
        AccountSpecs = accountSpecs;
        ExecutionLogSpecs = executionLogSpecs;

        OrderTrades = orderTrades;
        OrderTradeSpecs = orderTradeSpecs;
        StrategyEvaluations = strategyEvaluations;
        StrategyEvaluationSpecs = strategyEvaluationSpecs;
        MarketSnapshots = marketSnapshots;
        MarketSnapshotSpecs = marketSnapshotSpecs;
        CandleRecords = candleRecords;
        CandleRecordSpecs = candleRecordSpecs;
        TradingSchedules = tradingSchedules;
        TradingScheduleSpecs = tradingScheduleSpecs;

    }
public IRepository<TraderDbContext, OrderTrade, Guid> OrderTrades { get; }
    public ISpecificationRepository<OrderTrade, Guid> OrderTradeSpecs { get; }
    public IRepository<TraderDbContext, TraderAccount, Guid> Accounts { get; }
    public IRepository<TraderDbContext, Instrument, Guid> Instruments { get; }
    public IRepository<TraderDbContext, InstrumentOrderPreset, Guid> InstrumentOrderPresets { get; }
    public IRepository<TraderDbContext, TradingRun, Guid> TradingRuns { get; }
    public IRepository<TraderDbContext, Signal, Guid> Signals { get; }
    public IRepository<TraderDbContext, OrderIntent, Guid> OrderIntents { get; }
    public IRepository<TraderDbContext, BrokerOrder, Guid> BrokerOrders { get; }
    public IRepository<TraderDbContext, Position, Guid> Positions { get; }
    public IRepository<TraderDbContext, SchedulePlan, Guid> SchedulePlans { get; }
    public IRepository<TraderDbContext, ScheduledOrder, Guid> ScheduledOrders { get; }
    public IRepository<TraderDbContext, ExecutionLog, Guid> ExecutionLogs { get; }
    public IRepository<TraderDbContext, StrategyEvaluation, Guid> StrategyEvaluations { get; }
    public IRepository<TraderDbContext, MarketSnapshotRecord, Guid> MarketSnapshots { get; }
    public IRepository<TraderDbContext, CandleRecord, Guid> CandleRecords { get; }
    public IRepository<TraderDbContext, TradingSchedule, Guid> TradingSchedules { get; }

    public ISpecificationRepository<CandleRecord, Guid> CandleRecordSpecs { get; }
    public ISpecificationRepository<TradingRun, Guid> TradingRunSpecs { get; }
    public ISpecificationRepository<Signal, Guid> SignalSpecs { get; }
    public ISpecificationRepository<OrderIntent, Guid> OrderIntentSpecs { get; }
    public ISpecificationRepository<BrokerOrder, Guid> BrokerOrderSpecs { get; }
    public ISpecificationRepository<Position, Guid> PositionSpecs { get; }
    public ISpecificationRepository<Instrument, Guid> InstrumentSpecs { get; }
    public ISpecificationRepository<InstrumentOrderPreset, Guid> InstrumentOrderPresetSpecs { get; }
    public ISpecificationRepository<TraderAccount, Guid> AccountSpecs { get; }
    public ISpecificationRepository<ExecutionLog, Guid> ExecutionLogSpecs { get; }
    public ISpecificationRepository<StrategyEvaluation, Guid> StrategyEvaluationSpecs { get; }
    public ISpecificationRepository<MarketSnapshotRecord, Guid> MarketSnapshotSpecs { get; }
    public ISpecificationRepository<TradingSchedule, Guid> TradingScheduleSpecs { get; }
}