using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using People.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Infrastructure.Data.Configurations
{
    public class IndividualConfiguration : IEntityTypeConfiguration<Individual>
    {
        public void Configure(EntityTypeBuilder<Individual> builder)
        {
            builder.ToTable("Individuals", "people");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).HasDefaultValueSql("(newid())");

            builder.HasIndex(e => e.NationalCode).IsUnique();
            builder.HasIndex(e => e.UserId).IsUnique();

            builder.Property(e => e.Name).HasMaxLength(100);
            builder.Property(e => e.Family).HasMaxLength(100);
            builder.Property(e => e.FatherName).HasMaxLength(100);
            builder.Property(e => e.NationalCode).HasMaxLength(10);
            builder.Property(e => e.OneTimePassword).HasMaxLength(100);

            // Relationships
            builder.HasMany(e => e.IndividualDetails)
                   .WithOne(e => e.Individual)
                   .HasForeignKey(e => e.IndividualId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
