using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TraderServer.Domain.Entities;

namespace TraderServer.Infrastructure.Configurations
{
    public class OptionContractConfiguration : DataScopedEntityConfiguration<OptionContract>
    {
        public override void Configure(EntityTypeBuilder<OptionContract> builder)
        {
            base.Configure(builder);
            builder.ToTable("OptionContracts", "trader");

            builder.Property(r => r.FkStockId).IsRequired().IsUnicode(false);

            builder.HasIndex(p => new { p.FkStockId , p.DueDate  })
                 .HasDatabaseName("IX_OptionContract_FastLookup");
        }
    }
}
