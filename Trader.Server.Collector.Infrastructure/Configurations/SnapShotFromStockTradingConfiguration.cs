using Authorization.Domain.Entities;
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
    public class SnapShotFromStockTradingConfiguration : DataScopedEntityConfiguration<SnapShotFromStockTrading>
    {
        public override void Configure(EntityTypeBuilder<SnapShotFromStockTrading> builder)
        {

            base.Configure(builder); 
            builder.ToTable("SnapShotFromStockTrading", "collector");

            builder.Property(p => p.DateTime).IsRequired();

            builder.HasIndex(p => new { p.DateTime, p.FkStockId })
                   .HasDatabaseName("IX_SnapShotFromStockTrading_FastLookup");
        }
    }
}
