using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations
{
    public class ScheduledOrderConfiguration : BaseConfiguration<ScheduledOrder>
    {
        public override void Configure(EntityTypeBuilder<ScheduledOrder> builder)
        {
            base.Configure(builder);
            builder.ToTable("ScheduledOrders", "trader");

            builder.Property(x => x.PlanId).IsRequired();
            builder.Property(x => x.AccountId).IsRequired();

            builder.Property(x => x.SymbolIsin)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Side)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Mode)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Quantity).IsRequired();
            builder.Property(x => x.TotalValue).IsRequired();
            builder.Property(x => x.Time).IsRequired();

            builder.Property(x => x.Fired).IsRequired();
            builder.Property(x => x.FiredAt).IsRequired(false);

            builder.Property(x => x.Result)
                .IsRequired(false)
                .HasMaxLength(2000);

            builder.HasIndex(x => x.PlanId)
                .HasDatabaseName("IX_ScheduledOrder_PlanId");

            builder.HasIndex(x => x.AccountId)
                .HasDatabaseName("IX_ScheduledOrder_AccountId");
        }
    }
}