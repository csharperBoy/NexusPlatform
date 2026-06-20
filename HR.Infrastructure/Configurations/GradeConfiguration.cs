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
    public class GradeConfiguration : BaseConfiguration<Grade>
    {
        public override void Configure(EntityTypeBuilder<Grade> builder)
        {
            base.Configure(builder);
            builder.ToTable(" Grade", "hr");

            //builder.Property(p => p.Title).IsRequired().HasMaxLength(200);

            // هر پست متعلق به یک واحد سازمانی است
            //builder.HasOne(p => p.OrganizationUnit)
            //    .WithMany(ou => ou.Posts)
            //    .HasForeignKey(p => p.OrganizationUnitId)
            //    .OnDelete(DeleteBehavior.Cascade);

            // ایندکس برای جستجوی سریع پست‌ها در یک واحد
            //builder.HasIndex(p => p.OrganizationUnitId);
        }
    }
}
