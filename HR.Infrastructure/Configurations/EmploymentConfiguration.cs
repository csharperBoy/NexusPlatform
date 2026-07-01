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
    public class EmploymentConfiguration : BaseConfiguration<Employment>
    {
        public override void Configure(EntityTypeBuilder<Employment> builder)
        {
            base.Configure(builder);
            builder.ToTable(" Employment", "hr");


            builder.HasOne(d => d.EmploymentStatus).WithMany(p => p.Employments)
                .HasForeignKey(d => d.FkEmploymentStatusId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ Employment_ EmploymentStatus");

            builder.HasOne(d => d.EmploymentType).WithMany(p => p.Employments)
                .HasForeignKey(d => d.FkEmploymentTypeId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ Employment_ EmploymentType");
        }
    }
}
