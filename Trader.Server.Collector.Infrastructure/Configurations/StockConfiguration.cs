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
    public class StockConfiguration : DataScopedEntityConfiguration<Stock>
    {
        public override void Configure(EntityTypeBuilder<Stock> builder)
        {
            base.Configure(builder); 
            builder.ToTable("Stock", "trader");

            builder.Property(p => p.TypeOfMarket).HasConversion<byte>();

            builder.HasIndex(p => new { p.Isin, p.Title, p.OpenTime , p.TypeOfMarket })
                   .HasDatabaseName("IX_Stock_FastLookup");
            builder.HasIndex(p => new { p.Isin})
                  .HasDatabaseName("IX_Stock_Isin_FastLookup");
        }
    }
}
