using Authorization.Domain.Entities;
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

            builder.ToTable("Positions", "OrganizationManagement");

            builder.HasKey(ds => ds.Id);
            builder.HasIndex(ds => ds.Id).IsUnique();

           
        }
    }
}
