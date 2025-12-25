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
    public class PositionPersonConfiguration : AuditableEntityConfiguration<PositionPerson>
    {
        public override void Configure(EntityTypeBuilder<PositionPerson> builder)
        {
            base.Configure(builder);

            builder.ToTable("PositionPersons", "OrganizationManagement");

            builder.HasKey(p => p.Id);
            builder.HasIndex(p => p.Id).IsUnique();

            
        }
    }
}
