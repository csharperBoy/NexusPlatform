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
    public class PartiesRelationsConfiguration : BaseConfiguration<PartiesRelation>
    {
        public override void Configure(EntityTypeBuilder<PartiesRelation> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("PartiesRelations", "people");



            // relation 
            builder.HasOne(p => p.sourceParty)
                     .WithMany(pr => pr.sourceRealations)
                     .HasForeignKey(pr => pr.sourcePartyId)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.destinationParty)
                     .WithMany(pr => pr.destinationRealations)
                     .HasForeignKey(pr => pr.destinationPartyId)
                     .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
