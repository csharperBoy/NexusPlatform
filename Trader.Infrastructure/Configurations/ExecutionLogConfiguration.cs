using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trader.Domain.Entities;

namespace Trader.Infrastructure.Configurations
{
    public class ExecutionLogConfiguration : BaseConfiguration<ExecutionLog>
    {
        public override void Configure(EntityTypeBuilder<ExecutionLog> builder)
        {
            base.Configure(builder);
            builder.ToTable("ExecutionLogs", "trader");

            builder.Property(x => x.PlanId).IsRequired();
            builder.Property(x => x.OrderId).IsRequired(false);

            builder.Property(x => x.Timestamp).IsRequired();

            builder.Property(x => x.Level)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(4000);

            builder.Property(x => x.Code)
                .IsRequired(false);

            builder.HasIndex(x => x.PlanId)
                .HasDatabaseName("IX_ExecutionLog_PlanId");

            builder.HasIndex(x => x.Timestamp)
                .HasDatabaseName("IX_ExecutionLog_Timestamp");
        }
    }
}