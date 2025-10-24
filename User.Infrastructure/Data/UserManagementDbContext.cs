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
            modelBuilder.HasDefaultSchema("user"); // User Management Schema

            // Person Configuration
            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.NationalCode).IsUnique();

                entity.Property(p => p.NationalCode)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(p => p.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.LastName)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            // PersonProfile Configuration
            modelBuilder.Entity<PersonProfile>(entity =>
            {
                entity.HasKey(pp => pp.Id);

                // رابطه با Person
                entity.HasOne(pp => pp.Person)
                    .WithMany() // اگر Person به PersonProfile نیاز داشت، می‌توانید Collection اضافه کنید
                    .HasForeignKey(pp => pp.FkPersonId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ایندکس برای پرس و جوهای کارآمد
                entity.HasIndex(pp => new { pp.FkPersonId });

                entity.Property(pp => pp.Address).HasMaxLength(500);
                entity.Property(pp => pp.JobTitle).HasMaxLength(100);
                entity.Property(pp => pp.EducationLevel).HasMaxLength(50);
            });
        }
    }
}
