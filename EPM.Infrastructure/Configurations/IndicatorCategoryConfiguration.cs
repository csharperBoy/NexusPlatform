using Core.Infrastructure.Database.Configurations;
using EPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPM.Infrastructure.Configurations
{

    public class IndicatorCategoryConfiguration : BaseConfiguration<IndicatorCategory>
    {
        public override void Configure(EntityTypeBuilder<IndicatorCategory> entity)
        {
            base.Configure(entity);
            entity.ToTable("IndicatorCategory", "epm");

            entity.Property(e => e.FkCategoryId).HasColumnName("FkCategoryID");
            entity.Property(e => e.FkIndicatorId).HasColumnName("FkIndicatorID");

            entity.HasOne(d => d.FkCategory).WithMany(p => p.IndicatorCategories)
                .HasForeignKey(d => d.FkCategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndicatorCategory_Category");

            entity.HasOne(d => d.FkIndicator).WithMany(p => p.IndicatorCategories)
                .HasForeignKey(d => d.FkIndicatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndicatorCategory_Indicator");
        }
    }
}