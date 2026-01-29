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
    public class SnapShotFromOptionTradingConfiguration : DataScopedEntityConfiguration<SnapShotFromOptionTrading>
    {
        public override void Configure(EntityTypeBuilder<SnapShotFromOptionTrading> builder)
        {
            base.Configure(builder); 
            builder.ToTable("SnapShotFromOptionTrading", "collector");

            builder.Property(p => p.DateTime).IsRequired();

            builder.HasIndex(p => new { p.DateTime, p.FkOptionId })
                   .HasDatabaseName("IX_SnapShotFromOptionTrading_FastLookup");
        }
    }
}
