using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Infrastructure.Database.Configurations;
using HR.Domain.Entities;

namespace HR.Infrastructure.Configurations
{
    public class AssignmentConfiguration : BaseConfiguration<Assignment>
    {
        public override void Configure(EntityTypeBuilder<Assignment> builder)
        {
            base.Configure(builder);

            builder.ToTable("Assignments", "HR");

            builder.HasKey(ds => ds.Id);
            builder.HasIndex(ds => ds.Id).IsUnique();

        }
    }
}
