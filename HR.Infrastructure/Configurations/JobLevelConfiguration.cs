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
    public class JobLevelConfiguration : BaseConfiguration<JobLevel>
    {
        public override void Configure(EntityTypeBuilder<JobLevel> builder)
        {
            base.Configure(builder);
            builder.ToTable("JobLevel", "hr");

        }
    }
}
