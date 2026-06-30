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
   
    public class LegalPersonConfiguration : BaseConfiguration<LegalPerson>
    {
        public override void Configure(EntityTypeBuilder<LegalPerson> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("legalPersons", "people");

            builder.HasIndex(e => e.Title, "IX_Persons_FullName_FastLookup");

            builder.HasIndex(e => e.RegisterCode, "IX_Persons_Unique")
                .IsUnique()
                .HasFilter("([RegisterCode] IS NOT NULL)");

            builder.HasIndex(e => e.FkPartyId, "IX_legalPersons_fkPartyId");
            builder.HasOne(d => d.Party).WithMany(p => p.LegalPeople)
                .HasForeignKey(d => d.FkPartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_legalPersons_Parties");
        }
    }

}
