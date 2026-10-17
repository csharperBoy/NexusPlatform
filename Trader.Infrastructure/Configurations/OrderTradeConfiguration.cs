using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;
using Trader.Domain.ValueObjects;

namespace Trader.Infrastructure.Configurations;

public class OrderTradeConfiguration : BaseConfiguration<OrderTrade>
{
    public override void Configure(EntityTypeBuilder<OrderTrade> builder)
    {
        base.Configure(builder);
        builder.ToTable("OrderTrades", "trader");

        builder.Property(x => x.BrokerOrderId).IsRequired();
        builder.Property(x => x.TradeNumber).IsRequired();
        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Side).HasConversion<int>().IsRequired();

        builder.Property(x => x.Price)
            .HasConversion(p => p.Value, v => new Price(v))
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasConversion(q => q.Value, v => new Quantity(v))
            .IsRequired();

        builder.Property(x => x.TradeDate).IsRequired();
        builder.Property(x => x.IsCanceled).IsRequired().HasDefaultValue(false);
        builder.Property(x => x.CancelDateTime).IsRequired(false);
        builder.Property(x => x.CreatedAt).IsRequired();

        // Idempotency
        builder.HasIndex(x => new { x.BrokerOrderId, x.TradeNumber })
            .IsUnique()
            .HasDatabaseName("UX_OrderTrade_BrokerOrder_TradeNumber");

        builder.HasIndex(x => x.TradeNumber)
            .HasDatabaseName("IX_OrderTrade_TradeNumber");
    }
}