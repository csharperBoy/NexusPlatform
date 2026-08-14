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
    public class LocationContactConfiguration : BaseConfiguration<LocationContact>
    {
        public override void Configure(EntityTypeBuilder<LocationContact> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.ContactType).HasConversion<byte>();

            builder.ToTable("LocationContacts", "contact");
            builder.HasIndex(e => e.FkLocationId, "IX_LocationContacts_FkLocationId");
            //builder.HasOne(d => d.Location).WithMany(p => p.LocationContacts)
            //    .HasForeignKey(d => d.FkLocationId)
            //    .HasConstraintName("FK_LocationContacts_Locations");


        }

    }
}
