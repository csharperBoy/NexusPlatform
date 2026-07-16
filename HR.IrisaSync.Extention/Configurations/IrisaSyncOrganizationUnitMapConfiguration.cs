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
    public class IrisaSyncOrganizationUnitMapConfiguration : BaseConfiguration<IrisaSyncOrganizationUnitMap>
    {
        public override void Configure(EntityTypeBuilder<IrisaSyncOrganizationUnitMap> builder)
        {
            base.Configure(builder);

            builder.ToTable("IrisaSyncOrganizationUnitMap", "hr");


            builder.HasIndex(e => e.FkOrganizationUnitId, "IX_Assignments_OrganizationUnitId");


            builder.HasIndex(e => e.IrisaOrganizationUnitId, "IX_IrisaOrganizationUnitId_Unique")
                .IsUnique();

            builder.HasIndex(e => e.FkOrganizationUnitId, "IX_FkOrganizationUnitId_Unique")
                .IsUnique();
        }
    }
}
