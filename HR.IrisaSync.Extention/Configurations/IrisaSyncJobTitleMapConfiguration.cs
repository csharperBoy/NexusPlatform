using Core.Infrastructure.Database.Configurations;
using HR.Domain.Entities;
using HR.IrisaSync.Extention.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace HR.IrisaSync.Extention.Configurations
{
    public class IrisaSyncJobTitleMapConfiguration : BaseConfiguration<IrisaSyncJobTitleMap>
    {
        public override void Configure(EntityTypeBuilder<IrisaSyncJobTitleMap> builder)
        {
            base.Configure(builder);

            builder.ToTable("IrisaSyncJobTitleMap", "hr");


            builder.HasIndex(e => e.FkJobTitleId, "IX_Assignments_JobTitleId");

            builder.HasIndex(e => e.IrisaJobTitleId, "IX_IrisaJobTitleId_Unique")
                .IsUnique();

            builder.HasIndex(e => e.FkJobTitleId, "IX_FkJobTitleId_Unique")
                .IsUnique();

        }
    }
}
