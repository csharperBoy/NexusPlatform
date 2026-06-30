using Core.Domain.Interfaces;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using People.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Infrastructure.Configurations
{
    public class NaturalPersonProfileConfiguration : BaseConfiguration<NaturalPersonProfile>
    {
        public override void Configure(EntityTypeBuilder<NaturalPersonProfile> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("NaturalPersonProfiles", "people");
            builder.HasIndex(e => e.OwnerOrganizationUnitId, "IX_PersonProfile_OwnerOrgUnit");
            builder.HasIndex(e => e.OwnerPersonId, "IX_PersonProfile_OwnerPerson");
            builder.HasIndex(e => new { e.OwnerOrganizationUnitId, e.OwnerPersonId }, "IX_PersonProfile_ScopedLookup");
            builder.HasIndex(e => e.FkNaturalPersonId, "IX_PersonProfiles_FkPersonId");
            builder.HasOne(d => d.NaturalPerson).WithMany(p => p.NaturalPersonProfiles)
                .HasForeignKey(d => d.FkNaturalPersonId)
                .HasConstraintName("FK_NaturalPersonProfiles_naturalPersons");

        }
    }

}
