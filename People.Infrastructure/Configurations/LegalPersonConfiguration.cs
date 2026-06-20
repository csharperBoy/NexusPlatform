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
            builder.HasOne(p => p.party)
                     .WithMany(pr => pr.legalPersons)
                     .HasForeignKey(pr => pr.fkPartyId)
                     .OnDelete(DeleteBehavior.Cascade);

            // ایندکس ترکیبی طلایی برای چک کردن دسترسی
            builder.HasIndex(p => new { p.RegisterCode })
                   .HasDatabaseName("IX_Persons_FastLookup");
            builder.HasIndex(p => new { p.Title })
                 .HasDatabaseName("IX_Persons_FullName_FastLookup");

            // عدم ثبت دسترسی تکراری
            builder.HasIndex(p => new {
                p.RegisterCode
            })
            .HasDatabaseName("IX_Persons_Unique")
            .IsUnique();
        }
    }

}
