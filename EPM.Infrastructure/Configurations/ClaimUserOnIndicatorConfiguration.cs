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
    public class ClaimUserOnIndicatorConfiguration : BaseConfiguration<ClaimUserOnIndicator>
    {
        public override void Configure(EntityTypeBuilder<ClaimUserOnIndicator> entity)
        {
            base.Configure(entity);
            entity.ToTable("ClaimUserOnIndicator", "epm");


            entity.HasIndex(e => e.FkLkpClaimUserOnIndicatorId, "IX_ClaimUserOnIndicator_FkLkpClaimUserOnIndicatorID");

            entity.HasIndex(e => e.FkUserId, "IX_ClaimUserOnIndicator_FkUserID");

            entity.HasIndex(e => new { e.FkIndicatorId, e.FkLkpClaimUserOnIndicatorId, e.FkUserId }, "UNQ_ClaimUserIndicator").IsUnique();

            entity.Property(e => e.FkIndicatorId).HasColumnName("FkIndicatorID");
            entity.Property(e => e.FkLkpClaimUserOnIndicatorId).HasColumnName("FkLkpClaimUserOnIndicatorID");
            entity.Property(e => e.FkUserId).HasColumnName("FkUserID");

            entity.HasOne(d => d.FkIndicator).WithMany(p => p.ClaimUserOnIndicators)
                .HasForeignKey(d => d.FkIndicatorId)
                .HasConstraintName("FK_ClaimUserOnIndicator_Indicator");

            entity.HasOne(d => d.FkLkpClaimUserOnIndicator).WithMany(p => p.ClaimUserOnIndicators)
                .HasForeignKey(d => d.FkLkpClaimUserOnIndicatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClaimUserOnIndicator_LookUpValue");

            entity.HasOne(d => d.FkUser).WithMany(p => p.ClaimUserOnIndicators)
                .HasForeignKey(d => d.FkUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClaimUserOnIndicator_User");
        }
    }
}


modelBuilder.Entity<ClaimUserOnIndicator>(entity =>
{

});