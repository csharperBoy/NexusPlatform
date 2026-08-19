using Contact.Domain.Entities;
using Core.Infrastructure.Database.Configurations;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Infrastructure.Configurations
{
    public class ContactProfileConfiguration : BaseConfiguration<ContactProfile>
    {
        public override void Configure(EntityTypeBuilder<ContactProfile> builder)
        {
            base.Configure(builder);


            builder.ToTable("ContactProfiles", "contact");
            builder.HasMany(d => d.ContactItems).WithOne(p => p.ContactProfile)
                .HasForeignKey(d => d.ContactProfileId)
                .HasConstraintName("FK_ContactItems_ContactProfiles");


        }

    }
}
