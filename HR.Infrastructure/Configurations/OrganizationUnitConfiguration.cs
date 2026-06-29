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
    
    
    public class OrganizationUnitConfiguration : BaseConfiguration<OrganizationUnit>
    {
        public override void Configure(EntityTypeBuilder<OrganizationUnit> builder)
        {
            base.Configure(builder);

            

            builder.ToTable("OrganizationUnits", "hr");
            builder.HasIndex(e => e.Code, "IX_OrganizationUnits_Code").IsUnique();
            builder.HasIndex(e => e.Path, "IX_OrganizationUnits_Path");
            builder.Property(e => e.Code).HasMaxLength(50);
            builder.Property(e => e.Name).HasMaxLength(200);
            builder.Property(e => e.Path)
                .HasMaxLength(1000)
                .IsUnicode(false);

        }
    }
}
