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

    public class MapItemConfiguration : BaseConfiguration<MapItem>
    {
        public override void Configure(EntityTypeBuilder<MapItem> entity)
        {
            base.Configure(entity);
            entity.ToTable("MapItem", "epm");


            entity.ToTable(tb => tb.HasTrigger("trg_CheckLimitations"));

            entity.Property(e => e.FkCategoryId).HasColumnName("FkCategoryID");
            entity.Property(e => e.FkIndicatorCategoryId).HasColumnName("FkIndicatorCategoryID");
            entity.Property(e => e.FkIndicatorId).HasColumnName("FkIndicatorID");
            entity.Property(e => e.HighBound).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.LowBound).HasColumnType("decimal(18, 6)");
            entity.Property(e => e.Weight).HasColumnType("decimal(18, 6)");

            entity.HasOne(d => d.FkCategory).WithMany(p => p.MapItems)
            .HasForeignKey(d => d.FkCategoryId)
            .HasConstraintName("FK_MapItems_Category");

            entity.HasOne(d => d.FkIndicatorCategory).WithMany(p => p.MapItems)
            .HasForeignKey(d => d.FkIndicatorCategoryId)
            .HasConstraintName("FK_MapItems_IndicatorCategory");

            entity.HasOne(d => d.FkIndicator).WithMany(p => p.MapItems)
            .HasForeignKey(d => d.FkIndicatorId)
            .HasConstraintName("FK_MapItems_Indicator");
        }
    }
}
