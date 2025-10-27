using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using User.Domain.Entities;

namespace User.Infrastructure.Data
{
    public class UserManagementDbContext : DbContext
    {
        public UserManagementDbContext(DbContextOptions<UserManagementDbContext> options) : base(options) { }

        public DbSet<Person> Persons { get; set; } = null!;
        public DbSet<PersonProfile> PersonProfiles { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("user");

            // Person
            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(p => p.Id);

                // NationalCode به عنوان ValueObject
                entity.OwnsOne(p => p.NationalCode, nc =>
                {
                    nc.Property(x => x.Value)
                      .IsRequired()
                      .HasMaxLength(10)
                      .HasColumnName("NationalCode");
                });

                // FullName به عنوان ValueObject
                entity.OwnsOne(p => p.FullName, fn =>
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

                entity.Property(p => p.BirthDate);
                entity.Property(p => p.BirthPlace).HasMaxLength(200);
                entity.Property(p => p.FatherName).HasMaxLength(100);
                entity.Property(p => p.Gender);
                entity.Property(p => p.OneTimePassword).HasMaxLength(50);
            });

            // PersonProfile
            modelBuilder.Entity<PersonProfile>(entity =>
            {
                entity.HasKey(pp => pp.Id);

                entity.HasOne(pp => pp.Person)
                      .WithMany()
                      .HasForeignKey(pp => pp.FkPersonId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(pp => pp.Address).HasMaxLength(500);
                entity.Property(pp => pp.JobTitle).HasMaxLength(100);
                entity.Property(pp => pp.EducationLevel).HasMaxLength(50);
                entity.Property(pp => pp.MaritalStatus).IsRequired();

                // Phone به عنوان ValueObject
                entity.OwnsOne(pp => pp.Phone, phone =>
                {
                    phone.Property(x => x.Value)
                         .HasMaxLength(20)
                         .HasColumnName("Phone");
                });

                // Mobile به عنوان ValueObject
                entity.OwnsOne(pp => pp.Mobile, mobile =>
                {
                    mobile.Property(x => x.Value)
                          .HasMaxLength(20)
                          .HasColumnName("Mobile");
                });

                // Email به عنوان ValueObject
                entity.OwnsOne(pp => pp.Email, email =>
                {
                    email.Property(x => x.Value)
                         .HasMaxLength(200)
                         .HasColumnName("Email");
                });
            });
        }

    }
}
