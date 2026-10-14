using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Trader.Domain.Entities;
using Trader.Infrastructure.Configurations;

namespace Trader.Infrastructure.Data;

public class TraderDbContext : Base_DbContext
{
    public TraderDbContext(
        DbContextOptions<TraderDbContext> options,
        IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }

    public TraderDbContext(DbContextOptions<TraderDbContext> options)
        : base(options, new ServiceCollection().BuildServiceProvider())
    {
    }

    /* ═══════════ DbSets ═══════════ */

    // Existing
    public virtual DbSet<TraderAccount> Accounts { get; set; }
    public virtual DbSet<SchedulePlan> SchedulePlans { get; set; }
    public virtual DbSet<ScheduledOrder> ScheduledOrders { get; set; }
    public virtual DbSet<ExecutionLog> ExecutionLogs { get; set; }

    // New
    public virtual DbSet<Instrument> Instruments { get; set; }
    public virtual DbSet<InstrumentOrderPreset> InstrumentOrderPresets { get; set; }
    public virtual DbSet<TradingRun> TradingRuns { get; set; }
    public virtual DbSet<TradingRunSymbol> TradingRunSymbols { get; set; }
    public virtual DbSet<TradingRunAccount> TradingRunAccounts { get; set; }
    public virtual DbSet<Signal> Signals { get; set; }
    public virtual DbSet<OrderIntent> OrderIntents { get; set; }
    public virtual DbSet<BrokerOrder> BrokerOrders { get; set; }
    public virtual DbSet<Position> Positions { get; set; }

    /* ═══════════ Views / Triggers ═══════════ */

    public override void EnsureViews(CancellationToken cancellationToken = default)
    {
        // فعلاً viewی نداریم
    }

    public override void EnsureTriggers(CancellationToken cancellationToken = default)
    {
        // فعلاً triggerی نداریم
    }

    /* ═══════════ Model Creating ═══════════ */

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("trader");

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("trader"));

        // Existing
        modelBuilder.ApplyConfiguration(new TraderAccountConfiguration());
        modelBuilder.ApplyConfiguration(new SchedulePlanConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduledOrderConfiguration());
        modelBuilder.ApplyConfiguration(new ExecutionLogConfiguration());

        // New
        modelBuilder.ApplyConfiguration(new InstrumentConfiguration());
        modelBuilder.ApplyConfiguration(new InstrumentOrderPresetConfiguration());
        modelBuilder.ApplyConfiguration(new TradingRunConfiguration());
        modelBuilder.ApplyConfiguration(new TradingRunSymbolConfiguration());
        modelBuilder.ApplyConfiguration(new TradingRunAccountConfiguration());
        modelBuilder.ApplyConfiguration(new SignalConfiguration());
        modelBuilder.ApplyConfiguration(new OrderIntentConfiguration());
        modelBuilder.ApplyConfiguration(new BrokerOrderConfiguration());
        modelBuilder.ApplyConfiguration(new PositionConfiguration());
    }
}