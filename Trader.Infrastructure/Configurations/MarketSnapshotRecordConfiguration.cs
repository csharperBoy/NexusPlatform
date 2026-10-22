using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class MarketSnapshotRecordConfiguration : BaseConfiguration<MarketSnapshotRecord>
{
    public override void Configure(EntityTypeBuilder<MarketSnapshotRecord> builder)
    {
        base.Configure(builder);
        builder.ToTable("MarketSnapshots", "trader");

        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);
        builder.Property(x => x.SymbolName).HasMaxLength(100).IsRequired(false);
        builder.Property(x => x.CapturedAt).IsRequired();
        builder.Property(x => x.StateCode).HasMaxLength(10).IsRequired();

        // Price
        builder.Property(x => x.LastTradedPrice).IsRequired();
        builder.Property(x => x.ClosingPrice).IsRequired();
        builder.Property(x => x.PreviousDayClosingPrice).IsRequired();
        builder.Property(x => x.PriceVar).IsRequired();
        builder.Property(x => x.FirstTradedPrice).IsRequired();
        builder.Property(x => x.HighPrice).IsRequired();
        builder.Property(x => x.LowPrice).IsRequired();
        builder.Property(x => x.HighAllowedPrice).IsRequired();
        builder.Property(x => x.LowAllowedPrice).IsRequired();
        builder.Property(x => x.PriceTickSize).IsRequired();

        // Order Book
        builder.Property(x => x.BestBuyPrice).IsRequired();
        builder.Property(x => x.BestBuyQuantity).IsRequired();
        builder.Property(x => x.BestSellPrice).IsRequired();
        builder.Property(x => x.BestSellQuantity).IsRequired();

        // Pressure
        builder.Property(x => x.BuyRatio).HasPrecision(9, 4).IsRequired();
        builder.Property(x => x.SellRatio).HasPrecision(9, 4).IsRequired();
        builder.Property(x => x.MinValidBuyVolume).IsRequired();
        builder.Property(x => x.MaxValidBuyVolume).IsRequired();
        builder.Property(x => x.MinValidSellVolume).IsRequired();
        builder.Property(x => x.MaxValidSellVolume).IsRequired();

        // Trade Stats
        builder.Property(x => x.TotalTradeValue).IsRequired();
        builder.Property(x => x.TotalNumberOfTrades).IsRequired();
        builder.Property(x => x.TotalNumberOfSharesTraded).IsRequired();

        // Meta
        builder.Property(x => x.Source).HasMaxLength(50).IsRequired();

        // ─── Indexes ───
        builder.HasIndex(x => new { x.Isin, x.CapturedAt })
            .HasDatabaseName("IX_MarketSnapshot_Isin_Time");
        builder.HasIndex(x => x.CapturedAt)
            .HasDatabaseName("IX_MarketSnapshot_Time");
    }
}