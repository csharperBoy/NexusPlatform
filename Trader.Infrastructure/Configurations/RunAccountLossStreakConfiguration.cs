using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class RunAccountLossStreakConfiguration
    : BaseConfiguration<RunAccountLossStreak>
{
    public override void Configure(EntityTypeBuilder<RunAccountLossStreak> builder)
    {
        base.Configure(builder);
        builder.ToTable("RunAccountLossStreaks", "trader");

        builder.Property(x => x.RunId).IsRequired();
        builder.Property(x => x.AccountId).IsRequired();
        builder.Property(x => x.ConsecutiveLosses).IsRequired();

        builder.Property(x => x.LastLossAt).IsRequired(false);
        builder.Property(x => x.LastWinAt).IsRequired(false);
        builder.Property(x => x.CircuitBrokenAt).IsRequired(false);
        builder.Property(x => x.CooldownExpiresAt).IsRequired(false);

        builder.HasIndex(x => new { x.RunId, x.AccountId })
            .IsUnique()
            .HasDatabaseName("UX_RunAccountLossStreak_Run_Account");

        builder.HasIndex(x => x.CircuitBrokenAt)
            .HasDatabaseName("IX_RunAccountLossStreak_CircuitBroken")
            .HasFilter("[CircuitBrokenAt] IS NOT NULL");

        builder.HasIndex(x => x.AccountId)
            .HasDatabaseName("IX_RunAccountLossStreak_Account");
    }
}