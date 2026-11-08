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

        builder.Property(x => x.AllocationPercent)
            .HasPrecision(9, 4)
            .IsRequired()
            .HasDefaultValue(100m);   // برای رکوردهای موجود

        builder.HasIndex(x => new { x.TradingRunId, x.TraderAccountId })
            .IsUnique()
            .HasDatabaseName("UX_TradingRunAccount_Run_Account");

        builder.HasIndex(x => x.TraderAccountId)
            .HasDatabaseName("IX_TradingRunAccount_Account");
    }
}