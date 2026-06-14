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
    public class PersonProfileConfiguration : BaseConfiguration<PersonProfile>
    {
        public override void Configure(EntityTypeBuilder<PersonProfile> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("PersonProfiles", "authorization");

            
        }
    }

}
