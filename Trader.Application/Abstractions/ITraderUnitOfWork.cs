using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Trader.Domain.Entities;

namespace Trader.Application.Abstractions;

public interface ITraderUnitOfWork<TDbContext> : IUnitOfWork<TDbContext>
    where TDbContext : DbContext
{
    // ─── Direct Repositories ───
    IRepository<TDbContext, TraderAccount, Guid> Accounts { get; }
    IRepository<TDbContext, Instrument, Guid> Instruments { get; }
    IRepository<TDbContext, InstrumentOrderPreset, Guid> InstrumentOrderPresets { get; }
    IRepository<TDbContext, TradingRun, Guid> TradingRuns { get; }
    IRepository<TDbContext, Signal, Guid> Signals { get; }
    IRepository<TDbContext, OrderIntent, Guid> OrderIntents { get; }
    IRepository<TDbContext, BrokerOrder, Guid> BrokerOrders { get; }
    IRepository<TDbContext, Position, Guid> Positions { get; }
    IRepository<TDbContext, SchedulePlan, Guid> SchedulePlans { get; }
    IRepository<TDbContext, ScheduledOrder, Guid> ScheduledOrders { get; }
    IRepository<TDbContext, ExecutionLog, Guid> ExecutionLogs { get; }

    // ─── Specification Repositories ───
    ISpecificationRepository<TradingRun, Guid> TradingRunSpecs { get; }
    ISpecificationRepository<Signal, Guid> SignalSpecs { get; }
    ISpecificationRepository<OrderIntent, Guid> OrderIntentSpecs { get; }
    ISpecificationRepository<BrokerOrder, Guid> BrokerOrderSpecs { get; }
    ISpecificationRepository<Position, Guid> PositionSpecs { get; }
    ISpecificationRepository<TraderAccount, Guid> AccountSpecs { get; }
    ISpecificationRepository<ExecutionLog, Guid> ExecutionLogSpecs { get; }
}