using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class BrokerOrderConfiguration : BaseConfiguration<BrokerOrder>
{
    public override void Configure(EntityTypeBuilder<BrokerOrder> builder)
    {
        base.Configure(builder);
        builder.ToTable("BrokerOrders", "trader");

        builder.Property(x => x.OrderIntentId).IsRequired(false);
        builder.Property(x => x.RunId).IsRequired();
        builder.Property(x => x.TraderAccountId).IsRequired();
        builder.Property(x => x.BrokerOrderId).HasMaxLength(100).IsRequired(false);

        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Side).HasConversion<int>().IsRequired();
        builder.Property(x => x.State).HasConversion<int>().IsRequired();

        builder.Property(x => x.Price)
            .HasConversion(p => p.Value, v => new Trader.Domain.ValueObjects.Price(v))
            .HasPrecision(18, 4);

        builder.Property(x => x.Quantity)
            .HasConversion(q => q.Value, v => new Trader.Domain.ValueObjects.Quantity(v));

        builder.Property(x => x.FilledQuantity)
            .HasConversion(q => q.Value, v => new Trader.Domain.ValueObjects.Quantity(v));

        builder.Property(x => x.SubmittedAt).IsRequired();

        builder.HasIndex(x => new { x.TraderAccountId, x.BrokerOrderId })
            .HasDatabaseName("IX_BrokerOrder_Account_ExternalId");
        builder.HasIndex(x => new { x.RunId, x.State })
            .HasDatabaseName("IX_BrokerOrder_Run_State");
        builder.HasIndex(x => new { x.TraderAccountId, x.State })
            .HasDatabaseName("IX_BrokerOrder_Account_State");
    }
}