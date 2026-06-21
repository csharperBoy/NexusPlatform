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
    public class LocationConfiguration : BaseConfiguration<Location>
    {
        public override void Configure(EntityTypeBuilder<Location> builder)
        {
            base.Configure(builder);
            builder.ToTable("Location", "hr");

            //builder.Property(p => p.Title).IsRequired().HasMaxLength(200);

            // هر پست متعلق به یک واحد سازمانی است
            

            // ایندکس برای جستجوی سریع پست‌ها در یک واحد
            //builder.HasIndex(p => p.OrganizationUnitId);
        }
    }

}
