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
    public class EmploymentTypeConfiguration : BaseConfiguration<EmploymentType>
    {
        public override void Configure(EntityTypeBuilder<EmploymentType> builder)
        {
            base.Configure(builder);
            builder.ToTable(" EmploymentType", "hr");

        }
    }
}
