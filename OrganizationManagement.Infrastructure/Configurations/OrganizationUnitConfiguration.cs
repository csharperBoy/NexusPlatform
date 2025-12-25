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
    public class OrganizationUnitConfiguration : AuditableEntityConfiguration< OrganizationUnit>
    {
        public override void Configure(EntityTypeBuilder< OrganizationUnit> builder)
        {
            base.Configure(builder);

            builder.ToTable(" OrganizationUnits", "OrganizationManagement");

            builder.HasKey(r => r.Id);
            builder.HasIndex(r => r.Id).IsUnique();

            
        }
    }
}
