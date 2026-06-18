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
   
    public class LegalPersonConfiguration : BaseConfiguration<legalPersons>
    {
        public override void Configure(EntityTypeBuilder<legalPersons> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("legalPersons", "people");

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
