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
    public class PersonContactConfiguration : BaseConfiguration<PersonContact>
    {
        public override void Configure(EntityTypeBuilder<PersonContact> builder)
        {
            base.Configure(builder); 

            builder.Property(p => p.ContactType).HasConversion<byte>();

            builder.ToTable("PersonContacts", "people");


        }
    }
}
