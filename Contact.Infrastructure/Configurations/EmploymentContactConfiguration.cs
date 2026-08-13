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
    public class EmploymentContactConfiguration : BaseConfiguration<EmploymentContact>
    {
        public override void Configure(EntityTypeBuilder<EmploymentContact> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.ContactType).HasConversion<byte>();

            builder.ToTable("EmploymentContacts", "hr");
            builder.HasIndex(e => e.FkEmploymentId, "IX_PersonContacts_FkEmploymentId");
            //builder.HasOne(d => d.Employment).WithMany(p => p.EmploymentContacts)
            //    .HasForeignKey(d => d.FkEmploymentId)
            //    .HasConstraintName("FK_EmploymentContacts_Employments");


        }

    }
}
