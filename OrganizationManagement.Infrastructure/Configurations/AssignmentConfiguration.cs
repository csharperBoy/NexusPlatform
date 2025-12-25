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
    public class AssignmentConfiguration : AuditableEntityConfiguration<Assignment>
    {
        public override void Configure(EntityTypeBuilder<Assignment> builder)
        {
            base.Configure(builder);

            builder.ToTable("Assignments", "OrganizationManagement");

            builder.HasKey(ds => ds.Id);
            builder.HasIndex(ds => ds.Id).IsUnique();

        }
    }
}
