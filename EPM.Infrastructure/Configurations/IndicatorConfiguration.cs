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
    public class IndicatorConfiguration : BaseConfiguration<Indicator>
    {
        public override void Configure(EntityTypeBuilder<Indicator> entity)
        {
            base.Configure(entity);
            entity.ToTable("Indicator", "epm");

            entity.HasIndex(e => e.FkLkpDesirabilityId, "IX_Indicator_FkLkpDesirabilityID");

            entity.HasIndex(e => e.FkLkpFormId, "IX_Indicator_FkLkpFormID");

            entity.HasIndex(e => e.FkLkpManualityId, "IX_Indicator_FkLkpManualityID");

            entity.HasIndex(e => e.FkLkpMeasureId, "IX_Indicator_FkLkpMeasureID");

            entity.HasIndex(e => e.FkLkpPeriodId, "IX_Indicator_FkLkpPeriodID");

            entity.HasIndex(e => e.FkLkpTypeId, "IX_Indicator_FkLkpUnitID");

            entity.HasIndex(e => e.Code, "UNQ_Code").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.FkLkpDesirabilityId).HasColumnName("FkLkpDesirabilityID");
            entity.Property(e => e.FkLkpFormId).HasColumnName("FkLkpFormID");
            entity.Property(e => e.FkLkpManualityId).HasColumnName("FkLkpManualityID");
            entity.Property(e => e.FkLkpMeasureId).HasColumnName("FkLkpMeasureID");
            entity.Property(e => e.FkLkpPeriodId).HasColumnName("FkLkpPeriodID");
            entity.Property(e => e.FkLkpTypeId).HasColumnName("FkLkpTypeID");

            entity.HasOne(d => d.FkLkpDesirability).WithMany(p => p.IndicatorFkLkpDesirabilities)
                .HasForeignKey(d => d.FkLkpDesirabilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Indicator_LookUpValue_Desirability");

            entity.HasOne(d => d.FkLkpForm).WithMany(p => p.IndicatorFkLkpForms)
                .HasForeignKey(d => d.FkLkpFormId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Indicator_LookUpValue_Form");

            entity.HasOne(d => d.FkLkpManuality).WithMany(p => p.IndicatorFkLkpManualities)
                .HasForeignKey(d => d.FkLkpManualityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Indicator_LookUpValue_Manuality");

            entity.HasOne(d => d.FkLkpMeasure).WithMany(p => p.IndicatorFkLkpMeasures)
                .HasForeignKey(d => d.FkLkpMeasureId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Indicator_LookUpValue_Measure");

            entity.HasOne(d => d.FkLkpPeriod).WithMany(p => p.IndicatorFkLkpPeriods)
                .HasForeignKey(d => d.FkLkpPeriodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Indicator_LookUpValue_Period");

            entity.HasOne(d => d.FkLkpType).WithMany(p => p.IndicatorFkLkpTypes)
                .HasForeignKey(d => d.FkLkpTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Indicator_LookUpValue_Unit");
        }
    }
}
