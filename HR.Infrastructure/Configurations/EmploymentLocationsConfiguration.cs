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
    public class EmploymentLocationsConfiguration : BaseConfiguration<EmploymentLocations>
    {
        public override void Configure(EntityTypeBuilder<EmploymentLocations> builder)
        {
            base.Configure(builder);
            builder.ToTable(" EmploymentLocations", "hr");

            //builder.Property(p => p.Title).IsRequired().HasMaxLength(200);

            // هر پست متعلق به یک واحد سازمانی است
            builder.HasOne(p => p.location)
                .WithMany(ou => ou.employementLocations)
                .HasForeignKey(p => p.fkLocationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.employee)
                .WithMany(ou => ou.employementLocations)
                .HasForeignKey(p => p.fkEmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ایندکس برای جستجوی سریع پست‌ها در یک واحد
            //builder.HasIndex(p => p.OrganizationUnitId);
        }

    }
}
