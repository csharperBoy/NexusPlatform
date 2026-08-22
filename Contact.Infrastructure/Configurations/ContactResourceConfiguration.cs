using Contact.Domain.Entities;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Infrastructure.Configurations
{
    public class ContactResourceConfiguration : BaseConfiguration<ContactResource>
    {
        public override void Configure(EntityTypeBuilder<ContactResource> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.ContactType).HasConversion<byte>();

            builder.ToTable("ContactResources", "contact");


            builder.HasMany(d => d.Assignments).WithOne(p => p.ContactResource)
                .HasForeignKey(d => d.ContactResourceId)
                .HasConstraintName("FK_ContactResources_ContactProfiles");


        }

    }
}
