using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class RiskKillSwitchConfiguration : BaseConfiguration<RiskKillSwitch>
{
    public override void Configure(EntityTypeBuilder<RiskKillSwitch> builder)
    {
        base.Configure(builder);
        builder.ToTable("RiskKillSwitches", "trader");

        builder.Property(x => x.AccountId).IsRequired(false);
        builder.Property(x => x.ActivatedAt).IsRequired();
        builder.Property(x => x.ActivatedBy).HasMaxLength(256).IsRequired(false);
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired(false);
        builder.Property(x => x.DeactivatedAt).IsRequired(false);
        builder.Property(x => x.DeactivatedBy).HasMaxLength(256).IsRequired(false);

        // فعال‌ترین چک: "is there an active kill switch for this account?"
        builder.HasIndex(x => new { x.AccountId, x.DeactivatedAt })
            .HasDatabaseName("IX_RiskKillSwitch_Account_Active");

        // Global-only index (AccountId IS NULL)
        builder.HasIndex(x => x.DeactivatedAt)
            .HasDatabaseName("IX_RiskKillSwitch_Active");
    }
}