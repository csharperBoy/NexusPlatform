using Core.Infrastructure.Database.Configurations;
using HR.IrisaSync.Extention.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Configurations
{
    
    public class JobLevelMapConfiguration : BaseConfiguration<JobLevelMap>
    {
        public override void Configure(EntityTypeBuilder<JobLevelMap> builder)
        {
            base.Configure(builder);

            builder.ToTable("JobLevelMap", "hr");


            builder.HasIndex(e => e.FkJobLevelId, "IX_Assignments_JobLevelId");

            builder.HasIndex(e => e.IrisaJobLevelId, "IX_IrisaJobLevelId_Unique")
                .IsUnique();

            builder.HasIndex(e => e.FkJobLevelId, "IX_FkJobLevelId_Unique")
                .IsUnique();

        }
    }
}
