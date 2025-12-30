using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Infrastructure.Database.Configurations;
using OrganizationManagement.Domain.Entities;

namespace OrganizationManagement.Infrastructure.Services
{
    public class PositionConfiguration : AuditableEntityConfiguration<Position>
    {
        public override void Configure(EntityTypeBuilder<Position> builder)
        {
            base.Configure(builder);
            builder.ToTable("Positions", "organization");

            builder.Property(p => p.Title).IsRequired().HasMaxLength(200);

            // هر پست متعلق به یک واحد سازمانی است
            builder.HasOne(p => p.OrganizationUnit)
                .WithMany(ou => ou.Positions)
                .HasForeignKey(p => p.FkOrganizationUnitId)
                .OnDelete(DeleteBehavior.Cascade);

            // ایندکس برای جستجوی سریع پست‌ها در یک واحد
            builder.HasIndex(p => p.FkOrganizationUnitId);
        }
    }
}
