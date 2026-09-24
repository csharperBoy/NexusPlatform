using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations
{
    public class SchedulePlanConfiguration : BaseConfiguration<SchedulePlan>
    {
        public override void Configure(EntityTypeBuilder<SchedulePlan> builder)
        {
            base.Configure(builder);
            builder.ToTable("SchedulePlans", "trader");

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Date).IsRequired();
            builder.Property(x => x.Enabled).IsRequired();
            builder.Property(x => x.AutoLoginAt).IsRequired();
            builder.Property(x => x.AutoRefreshAt).IsRequired();

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.LastMessage)
                .IsRequired(false)
                .HasMaxLength(2000);

            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.UpdatedAt).IsRequired(false);

            // ─── Orders (aggregate داخلی) ───
            builder.Metadata
                .FindNavigation(nameof(SchedulePlan.Orders))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(x => x.Orders)
                .WithOne()
                .HasForeignKey(o => o.PlanId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SchedulePlan_Orders");

            // ─── Indexها ───
            builder.HasIndex(x => x.Date)
                .HasDatabaseName("IX_SchedulePlan_Date");

            builder.HasIndex(x => new { x.Date, x.Enabled })
                .HasDatabaseName("IX_SchedulePlan_Date_Enabled");
        }
    }
}