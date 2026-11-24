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

    public class LookUpValueConfiguration : BaseConfiguration<LookUpValue>
    {
        public override void Configure(EntityTypeBuilder<LookUpValue> entity)
        {
            base.Configure(entity);
            entity.ToTable("LookUpValue" , "epm");

            entity.HasIndex(e => e.FkLookUpId, "IX_LookUpValue_FkLookUpID");

            entity.Property(e => e.FkLookUpId).HasColumnName("FkLookUpID");

            entity.HasOne(d => d.FkLookUp).WithMany(p => p.LookUpValues)
                .HasForeignKey(d => d.FkLookUpId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LookUpValue_LookUp");
        }
    }
}

