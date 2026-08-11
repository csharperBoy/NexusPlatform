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
    public class PostLocationsConfiguration : BaseConfiguration<PostLocation>
    {
        public override void Configure(EntityTypeBuilder<PostLocation> builder)
        {
            base.Configure(builder);
            builder.ToTable("PostLocations", "hr");
            builder.HasIndex(e => e.FkPostId, "IX_PostLocations_fkPostId");
            builder.HasIndex(e => e.FkLocationId, "IX_PostLocations_fkLocationId");


            builder.HasOne(d => d.Post).WithMany(p => p.PostLocations)
                .HasForeignKey(d => d.FkPostId)
                .HasConstraintName("FK_PostLocations_Post");

            builder.HasOne(d => d.Location).WithMany(p => p.PostLocations)
                .HasForeignKey(d => d.FkLocationId)
                .HasConstraintName("FK_PostLocations_Location");
        }

    }
}
