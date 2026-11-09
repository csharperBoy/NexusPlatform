using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class TradingRunAccountConfiguration : BaseConfiguration<TradingRunAccount>
{
    public override void Configure(EntityTypeBuilder<TradingRunAccount> builder)
    {
        base.Configure(builder);
        builder.ToTable("TradingRunAccounts", "trader");

        builder.Property(x => x.TradingRunId).IsRequired();
        builder.Property(x => x.TraderAccountId).IsRequired();

        // ─── CapitalAllocation (Owned) ───
        builder.OwnsOne(x => x.CapitalAllocation, ab =>
        {
            ab.Property(p => p.Mode)
                .HasConversion<int>()
                .HasColumnName("CapitalMode")
                .IsRequired();

            ab.OwnsOne(p => p.FixedAmount, mb =>
            {
                mb.Property(m => m.Amount)
                    .HasColumnName("FixedCapitalAmount")
                    .HasPrecision(18, 4);
                mb.Property(m => m.Currency)
                    .HasColumnName("FixedCapitalCurrency")
                    .HasMaxLength(10);
            });
            ab.Navigation(p => p.FixedAmount).IsRequired(false);

            ab.Property(p => p.PercentOfBuyPower)
                .HasColumnName("PercentOfBuyPower")
                .HasPrecision(9, 4)
                .IsRequired(false);
        });

        builder.HasIndex(x => new { x.TradingRunId, x.TraderAccountId })
            .IsUnique()
            .HasDatabaseName("UX_TradingRunAccount_Run_Account");

        builder.HasIndex(x => x.TraderAccountId)
            .HasDatabaseName("IX_TradingRunAccount_Account");
    }
}