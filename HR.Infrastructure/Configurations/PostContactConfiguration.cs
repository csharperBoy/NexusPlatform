using Core.Infrastructure.Database.Configurations;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Infrastructure.Configurations
{
    public class PostContactConfiguration : BaseConfiguration<PostContact>
    {
        public override void Configure(EntityTypeBuilder<PostContact> builder)
        {
            base.Configure(builder);

            builder.Property(p => p.ContactType).HasConversion<byte>();

            builder.ToTable("PostContacts", "hr");
            builder.HasIndex(e => e.FkPostId, "IX_PersonContacts_FkPostId");
            builder.HasOne(d => d.Post).WithMany(p => p.PostContacts)
                .HasForeignKey(d => d.FkPostId)
                .HasConstraintName("FK_PostContacts_Posts");


        }

    }
}
