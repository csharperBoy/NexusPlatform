using Core.Domain.Interfaces;
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
    
    public class StockFundPortfolioConfiguration : AuditableEntityConfiguration<StockFundPortfolio>
    {
        public override void Configure(EntityTypeBuilder<StockFundPortfolio> builder)
        {
            base.Configure(builder);
            builder.ToTable("StockFundPortfolio", "trader");

            builder.HasIndex(p => new { p.StockFundId })
                  .HasDatabaseName("IX_StockFundPortfolio_FastLookup");

        }
    }
}
