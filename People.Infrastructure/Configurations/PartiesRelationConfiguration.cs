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
    public class PartiesRelationConfiguration : BaseConfiguration<PartiesRelation>
    {
        public override void Configure(EntityTypeBuilder<PartiesRelation> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("PartiesRelations", "people");
            builder.HasIndex(e => e.FkDestinationPartyId, "IX_PartiesRelations_destinationPartyId");
            builder.HasIndex(e => e.FkSourcePartyId, "IX_PartiesRelations_sourcePartyId");
            builder.HasOne(d => d.DestinationParty).WithMany(p => p.PartiesRelationFkDestinationParties)
                .HasForeignKey(d => d.FkDestinationPartyId)
                .HasConstraintName("FK_PartiesRelations_Parties1");

            builder.HasOne(d => d.SourceParty).WithMany(p => p.PartiesRelationFkSourceParties)
                .HasForeignKey(d => d.FkSourcePartyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PartiesRelations_Parties");

        }
    }
}
