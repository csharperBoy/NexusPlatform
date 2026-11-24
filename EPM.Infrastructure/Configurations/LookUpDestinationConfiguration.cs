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

    public class LookUpDestinationConfiguration : BaseConfiguration<LookUpDestination>
    {
        public override void Configure(EntityTypeBuilder<LookUpDestination> entity)
        {
            base.Configure(entity);
            entity.ToTable("LookUpDestination", "epm");


            entity.HasIndex(e => e.FkLookUpId, "IX_LookUpDestination_FkLookUpID");

            entity.HasIndex(e => new { e.ColumnName, e.TableName, e.FkLookUpId }, "UNQ_LookUpDestination").IsUnique();

            entity.Property(e => e.ColumnName)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.FkLookUpId).HasColumnName("FkLookUpID");
            entity.Property(e => e.TableName)
                .HasMaxLength(300)
                .IsUnicode(false);

            entity.HasOne(d => d.FkLookUp).WithMany(p => p.LookUpDestinations)
                .HasForeignKey(d => d.FkLookUpId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LookUpDestination_LookUp");
        }
    }
}
