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
    public class IndicatorValueConfiguration : BaseConfiguration<IndicatorValue>
    {
        public override void Configure(EntityTypeBuilder<IndicatorValue> entity)
        {
            base.Configure(entity);
            entity.ToTable("IndicatorValue", "epm");

            entity.HasIndex(e => new { e.FkIndicatorId, e.DateTime, e.FkLkpValueTypeId, e.FkLkpShiftId }, "IX_IndicatorValue").IsUnique();

            entity.HasIndex(e => e.FkLkpShiftId, "IX_IndicatorValue_FkLkpShiftID");

            entity.HasIndex(e => e.FkLkpValueTypeId, "IX_IndicatorValue_FkLkpValueTypeID");

            entity.HasIndex(e => new { e.FkIndicatorId, e.DateTime }, "idx_indicatorvalue_FkIndicatorID_DateTime_FlgLogicalDelete");

            entity.HasIndex(e => e.FkLkpShiftId, "idx_indicatorvalue_FkLkpShiftID");

            entity.HasIndex(e => e.FkLkpValueTypeId, "idx_indicatorvalue_FkLkpValueType");

            entity.Property(e => e.FkIndicatorId).HasColumnName("FkIndicatorID");
            entity.Property(e => e.FkLkpShiftId).HasColumnName("FkLkpShiftID");
            entity.Property(e => e.FkLkpValueTypeId).HasColumnName("FkLkpValueTypeID");
            entity.Property(e => e.Value).HasColumnType("decimal(24, 6)");
            entity.Property(e => e.ValueCumulative).HasColumnType("decimal(24, 6)");

            entity.HasOne(d => d.FkIndicator).WithMany(p => p.IndicatorValues)
                .HasForeignKey(d => d.FkIndicatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndicatorValue_Indicator");

            entity.HasOne(d => d.FkLkpShift).WithMany(p => p.IndicatorValueFkLkpShifts)
                .HasForeignKey(d => d.FkLkpShiftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndicatorValue_LookUpValue_Shift");

            entity.HasOne(d => d.FkLkpValueType).WithMany(p => p.IndicatorValueFkLkpValueTypes)
                .HasForeignKey(d => d.FkLkpValueTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IndicatorValue_LookUpValue_Type");
        }
    }
}

