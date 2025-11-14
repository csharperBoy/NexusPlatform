using Authorization.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Configurations
{
    public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
    {
        public void Configure(EntityTypeBuilder<Resource> b)
        {
            b.ToTable("Resources", "authorization");

            b.HasKey(x => x.Id);
            b.Property(x => x.Code).IsRequired().HasMaxLength(255);
            b.Property(x => x.Name).IsRequired().HasMaxLength(255);
            b.Property(x => x.Type).IsRequired().HasMaxLength(50);
            b.Property(x => x.Metadata).HasColumnType("nvarchar(max)").IsRequired(false);
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasIndex(x => x.Code).IsUnique();

            b.HasOne(x => x.Parent)
             .WithMany(x => x.Children)
             .HasForeignKey(x => x.ParentId)
             .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
