using Core.Infrastructure.Data;
using Core.Infrastructure.Database;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Trader.Domain.Entities;
using Trader.Infrastructure.Configurations;

namespace Trader.Infrastructure.Data
{
    public class TraderDbContext : Base_DbContext
    {
        public TraderDbContext(
            DbContextOptions<TraderDbContext> options,
            IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
        }

        // این constructor برای ابزارهای design-time (dotnet ef)
        public TraderDbContext(DbContextOptions<TraderDbContext> options)
            : base(options, new ServiceCollection().BuildServiceProvider())
        {
        }

        /* ═══════════ DbSets ═══════════ */

        public virtual DbSet<TraderAccount> Accounts { get; set; }
        public virtual DbSet<TraderSymbol> Symbols { get; set; }
        public virtual DbSet<SchedulePlan> SchedulePlans { get; set; }
        public virtual DbSet<ScheduledOrder> ScheduledOrders { get; set; }
        public virtual DbSet<ExecutionLog> ExecutionLogs { get; set; }

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
            modelBuilder.ApplyConfiguration(new TraderAccountConfiguration());
            modelBuilder.ApplyConfiguration(new TraderSymbolConfiguration());
            modelBuilder.ApplyConfiguration(new SchedulePlanConfiguration());
            modelBuilder.ApplyConfiguration(new ScheduledOrderConfiguration());
            modelBuilder.ApplyConfiguration(new ExecutionLogConfiguration());
        }
    }
}