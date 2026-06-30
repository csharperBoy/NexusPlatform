using Core.Domain.Interfaces;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using People.Domain.Entities;

namespace People.Infrastructure.Configurations
{   
    public class PartyConfiguration : BaseConfiguration<Party>
    {
        public override void Configure(EntityTypeBuilder<Party> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("Parties", "people");

        }
    }
}
