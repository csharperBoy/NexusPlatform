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
   
    public class PartiesConfiguration : BaseConfiguration<Parties>
    {
        public override void Configure(EntityTypeBuilder<Parties> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("Parties", "people");

          

            // relation 
            builder.HasMany(p => p.legalPersons)
                     .WithOne(pr => pr.party)
                     .HasForeignKey(pr => pr.fkPartyId)
                     .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.NaturalPersons)
                    .WithOne(pr => pr.Party)
                    .HasForeignKey(pr => pr.fkPartyId)
                    .OnDelete(DeleteBehavior.Cascade);

        }
    }

}
