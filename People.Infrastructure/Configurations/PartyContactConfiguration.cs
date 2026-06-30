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
    public class PartyContactConfiguration : BaseConfiguration<PartyContact>
    {
        public override void Configure(EntityTypeBuilder<PartyContact> builder)
        {
            base.Configure(builder); 

            builder.Property(p => p.ContactType).HasConversion<byte>();

            builder.ToTable("PartyContacts", "people");
            builder.HasIndex(e => e.OwnerOrganizationUnitId, "IX_PersonContact_OwnerOrgUnit");
            builder.HasIndex(e => e.OwnerPersonId, "IX_PersonContact_OwnerPerson");
            builder.HasIndex(e => new { e.OwnerOrganizationUnitId, e.OwnerPersonId }, "IX_PersonContact_ScopedLookup");
            builder.HasIndex(e => e.FkPartyId, "IX_PersonContacts_FkPartyId");
            builder.HasOne(d => d.Party).WithMany(p => p.PartyContacts)
                .HasForeignKey(d => d.FkPartyId)
                .HasConstraintName("FK_PartyContacts_Parties");


        }
    }
}
