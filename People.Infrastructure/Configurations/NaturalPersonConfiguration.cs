using Core.Domain.Interfaces;
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
    public class NaturalPersonConfiguration : BaseConfiguration<naturalPerson>
    {
        public override void Configure(EntityTypeBuilder<naturalPerson> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("naturalPerson", "people");

            // 1. تنظیمات و ایندکس‌های مربوط به NationalCode
            builder.OwnsOne(p => p.NationalCode, nc =>
            {
                nc.Property(x => x.Value)
                  .IsRequired()
                  .HasMaxLength(10)
                  .HasColumnName("NationalCode");

                // ✅ ایندکس و یونیک بودن باید اینجا و روی Value تعریف شود
                nc.HasIndex(x => x.Value)
                  .HasDatabaseName("IX_Persons_Unique_NationalCode")
                  .IsUnique();
            });

            // 2. تنظیمات و ایندکس‌های مربوط به FullName
            builder.OwnsOne(p => p.FullName, fn =>
            {
                fn.Property(x => x.FirstName)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasColumnName("FirstName");

                fn.Property(x => x.LastName)
                  .IsRequired()
                  .HasMaxLength(100)
                  .HasColumnName("LastName");

                // ✅ ایندکس روی نام و نام خانوادگی باید اینجا تعریف شود
                fn.HasIndex(x => new { x.FirstName, x.LastName })
                  .HasDatabaseName("IX_Persons_FullName_FastLookup");
            });

            // 3. روابط (Relations)
            builder.HasMany(p => p.Profiles)
                     .WithOne(pr => pr.Person)
                     .HasForeignKey(pr => pr.FkPersonId)
                     .OnDelete(DeleteBehavior.Cascade);
        }
    }

}
