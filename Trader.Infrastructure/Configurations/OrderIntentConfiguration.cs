using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations;

public class OrderIntentConfiguration : BaseConfiguration<OrderIntent>
{
    public override void Configure(EntityTypeBuilder<OrderIntent> builder)
    {
        base.Configure(builder);
        builder.ToTable("OrderIntents", "trader");

        builder.Property(x => x.RunId).IsRequired();
        builder.Property(x => x.TraderAccountId).IsRequired();
        builder.Property(x => x.SourceSignalId).IsRequired(false);

        builder.Property(x => x.Isin).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Side).HasConversion<int>().IsRequired();
        builder.Property(x => x.OrderMode).HasConversion<int>().IsRequired();
        builder.Property(x => x.State).HasConversion<int>().IsRequired();

        builder.Property(x => x.Price)
            .HasConversion(p => p.Value, v => new Trader.Domain.ValueObjects.Price(v))
            .HasPrecision(18, 4);

        builder.Property(x => x.Quantity)
            .HasConversion(q => q.Value, v => new Trader.Domain.ValueObjects.Quantity(v));

        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.FailureReason).HasMaxLength(1000).IsRequired(false);

        builder.Property(x => x.DispatchedAt).IsRequired(false);
        builder.Property(x => x.CompletedAt).IsRequired(false);

        builder.HasIndex(x => new { x.RunId, x.State })
            .HasDatabaseName("IX_OrderIntent_Run_State");
        builder.HasIndex(x => new { x.TraderAccountId, x.Isin })
            .HasDatabaseName("IX_OrderIntent_Account_Isin");
    }
}