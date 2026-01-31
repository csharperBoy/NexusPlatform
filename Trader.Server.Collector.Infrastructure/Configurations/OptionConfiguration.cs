using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Server.Collector.Domain.Entities;

namespace Trader.Server.Collector.Infrastructure.Configurations
{
    public class OptionConfiguration : DataScopedEntityConfiguration<Option>
    {
        public override void Configure(EntityTypeBuilder<Option> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("Options", "trader");

            builder.Property(p => p.Side).HasConversion<byte>();

            // ایندکس ترکیبی طلایی برای چک کردن دسترسی
            builder.HasIndex(p => new { p.Side, p.FkOptionContractId ,p.DuePrice})
                   .HasDatabaseName("IX_Option_FastLookup");
            builder.HasIndex(p => new { p.Title })
                 .HasDatabaseName("IX_Option_Title_FastLookup");
        }
    }
}
