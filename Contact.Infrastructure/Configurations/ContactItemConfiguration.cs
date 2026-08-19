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
    public class ContactItemConfiguration : BaseConfiguration<ContactItem>
    {
        public override void Configure(EntityTypeBuilder<ContactItem> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.ContactType).HasConversion<byte>();

            builder.ToTable("ContactItems", "contact");
            builder.HasIndex(e => e.ContactProfileId, "IX_ContactItems_ContactProfileId");
            builder.HasOne(d => d.ContactProfile).WithMany(p => p.ContactItems)
                .HasForeignKey(d => d.ContactProfileId)
                .HasConstraintName("FK_ContactItems_ContactProfiles");


        }

    }
}
