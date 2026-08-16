using Core.Domain.Interfaces;
using Core.Infrastructure.Database.Configurations;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Infrastructure.Configurations
{
    public class EmploymentLocationsConfiguration : BaseConfiguration<EmploymentLocation>
    {
        public override void Configure(EntityTypeBuilder<EmploymentLocation> builder)
        {
            base.Configure(builder);
            builder.ToTable("EmploymentLocations", "hr");
            builder.HasIndex(e => e.FkEmploymentId, "IX_ EmploymentLocations_fkEmploymentId");
            builder.HasIndex(e => e.FkLocationId, "IX_ EmploymentLocations_fkLocationId");
            

            builder.HasOne(d => d.Employment).WithMany(p => p.EmploymentLocations)
                .HasForeignKey(d => d.FkEmploymentId)
                .HasConstraintName("FK_ EmploymentLocations_ Employment");

            builder.HasOne(d => d.Location).WithMany(p => p.EmploymentLocations)
                .HasForeignKey(d => d.FkLocationId)
                .HasConstraintName("FK_ EmploymentLocations_Location");
        }

    }
}
