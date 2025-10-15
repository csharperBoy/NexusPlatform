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
    public class IndividualDetailConfiguration : IEntityTypeConfiguration<IndividualDetail>
    {
        public void Configure(EntityTypeBuilder<IndividualDetail> builder)
        {
            builder.ToTable("IndividualDetails", "people");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.Property(e => e.IsCurrentVersion).HasDefaultValue(true);
            builder.Property(e => e.VersionStartDate).HasDefaultValueSql("(getutcdate())");
            builder.Property(e => e.Enablity).HasDefaultValue(true);
            builder.Property(e => e.Visiblity).HasDefaultValue(true);
            builder.Property(e => e.Remove).HasDefaultValue(false);

            builder.Property(e => e.Mobile).HasMaxLength(15);
            builder.Property(e => e.Phone).HasMaxLength(15);
            builder.Property(e => e.EmergencyMobile).HasMaxLength(15);
            builder.Property(e => e.Mail).HasMaxLength(256);
            builder.Property(e => e.Address).HasMaxLength(500);
            builder.Property(e => e.PasswordEmergency).HasMaxLength(100);

            // Self-referencing relationships for versioning
            builder.HasOne(d => d.PreviousVersion)
                   .WithMany(p => p.InversePreviousVersion)
                   .HasForeignKey(d => d.PreviousVersionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.NextVersion)
                   .WithMany(p => p.InverseNextVersion)
                   .HasForeignKey(d => d.NextVersionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
