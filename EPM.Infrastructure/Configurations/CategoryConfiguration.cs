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
    public class CategoryConfiguration : BaseConfiguration<Category>
    {
        public override void Configure(EntityTypeBuilder<Category> entity)
        {
            base.Configure(entity);
            entity.ToTable("Category", "epm");

            entity.HasIndex(e => e.FklkpTypeId, "IX_Category_FKLkpTypeID");

            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FkParentId).HasColumnName("FkParentID");
            entity.Property(e => e.FklkpTypeId).HasColumnName("FKLkpTypeID");
            entity.Property(e => e.Title).HasMaxLength(1500);

            entity.HasOne(d => d.FkParent).WithMany(p => p.InverseFkParent)
                .HasForeignKey(d => d.FkParentId)
                .HasConstraintName("FK_Category_Category");

            entity.HasOne(d => d.FklkpType).WithMany(p => p.Categories)
                .HasForeignKey(d => d.FklkpTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Category_LookUpValue");
        }
    }
}