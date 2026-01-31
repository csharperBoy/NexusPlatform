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
    
    public class BrokerageAccountConfiguration : DataScopedEntityConfiguration<BrokerageAccount>
    {
        public override void Configure(EntityTypeBuilder<BrokerageAccount> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("BrokerageAccount", "trader");

            builder.Property(p => p.Platform).HasConversion<byte>();

            // ایندکس ترکیبی طلایی برای چک کردن دسترسی
            builder.HasIndex(p => new { p.Platform, p.FkUserId })
                   .HasDatabaseName("IX_Option_FastLookup");
        }
    }
}
