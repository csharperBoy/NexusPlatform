using Core.Infrastructure.Database.Configurations;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace HR.Infrastructure.Configurations
{
    public class AssignmentConfiguration : BaseConfiguration<Assignment>
    {
        public override void Configure(EntityTypeBuilder<Assignment> builder)
        {
            base.Configure(builder);

            builder.ToTable("Assignments", "hr");
            builder.HasIndex(e => e.FkEmploymentId, "IX_Assignments_EmploymentId");
            builder.HasIndex(e => e.FkPostId, "IX_Assignments_PostId");

            builder.HasOne(d => d.Employment).WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.FkEmploymentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Assignments_ Employment");

            builder.HasOne(d => d.Post).WithMany(p => p.Assignments)
                    .HasForeignKey(d => d.FkPostId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Assignments_Post");
        }
    }
}
