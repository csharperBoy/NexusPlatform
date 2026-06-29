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
    public class PostConfiguration : BaseConfiguration<Post>
    {
        public override void Configure(EntityTypeBuilder<Post> builder)
        {
            base.Configure(builder);
            builder.ToTable("Post", "hr");
            builder.HasIndex(e => e.FkCostCenterId, "IX_Post_CostCenterId");
            builder.HasIndex(e => e.FkGradeId, "IX_Post_GradeId");
            builder.HasIndex(e => e.FkJobLevelId, "IX_Post_JobLevelId");
            builder.HasIndex(e => e.FkJobTitleId, "IX_Post_JobTitleId");
            builder.HasIndex(e => e.FkOrganizationUnitId, "IX_Post_OrganizationUnitId");

            builder.HasOne(d => d.CostCenter).WithMany(p => p.Posts)
                .HasForeignKey(d => d.FkCostCenterId)
                .HasConstraintName("FK_Post_ CostCenter");

            builder.HasOne(d => d.Grade).WithMany(p => p.Posts)
                .HasForeignKey(d => d.FkGradeId)
                .HasConstraintName("FK_Post_ Grade");

            builder.HasOne(d => d.JobLevel).WithMany(p => p.Posts)
                .HasForeignKey(d => d.FkJobLevelId)
                .HasConstraintName("FK_Post_JobLevel");

            builder.HasOne(d => d.JobTitle).WithMany(p => p.Posts)
                .HasForeignKey(d => d.FkJobTitleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Post_JobTitle");

            builder.HasOne(d => d.OrganizationUnit).WithMany(p => p.Posts)
                .HasForeignKey(d => d.FkOrganizationUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Post_OrganizationUnits");

        }
    }
}
