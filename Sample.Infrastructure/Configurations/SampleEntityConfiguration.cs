using Core.Domain.Interfaces;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Infrastructure.Configurations
{
    public class SampleEntityConfiguration : DataScopedEntityConfiguration<SampleEntity>
    {
        public override void Configure(EntityTypeBuilder<SampleEntity> builder)
        {
            base.Configure(builder);
            builder.ToTable("SampleEntity", "Sample");

            builder.HasKey(e => e.Id); // کلید اصلی
            builder.Property(e => e.property1)
                  .IsRequired()
                  .HasMaxLength(200); // محدودیت طول


            builder.HasIndex(p => new { p.property1 , p.Id})
                  .HasDatabaseName("IX_SampleEntity_FastLookup");

        }
    }
}
