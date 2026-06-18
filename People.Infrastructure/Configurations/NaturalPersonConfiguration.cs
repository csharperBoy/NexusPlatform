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

            builder.OwnsOne(p => p.NationalCode, nc =>
            {
                nc.Property(x => x.Value)
                  .IsRequired()
                  .HasMaxLength(10)
                  .HasColumnName("NationalCode");
            });

            // FullName به عنوان ValueObject
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
            });

            // relation 
            builder.HasMany(p => p.Profiles)
                     .WithOne(pr => pr.Person)
                     .HasForeignKey(pr => pr.FkPersonId)
                     .OnDelete(DeleteBehavior.Cascade);

            // ایندکس ترکیبی طلایی برای چک کردن دسترسی
            builder.HasIndex(p => new { p.NationalCode })
                   .HasDatabaseName("IX_Persons_FastLookup");
            builder.HasIndex(p => new { p.FullName })
                 .HasDatabaseName("IX_Persons_FullName_FastLookup");

            // عدم ثبت دسترسی تکراری
            builder.HasIndex(p => new {
                p.NationalCode
            })
            .HasDatabaseName("IX_Persons_Unique")
            .IsUnique();
        }
    }

}
