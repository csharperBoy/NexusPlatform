using Core.Domain.Common;
using Core.Infrastructure.Database.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Authorization.Infrastructure.Identity;


namespace Authorization.Infrastructure.Data
{
    // فقط Role-centric: جدول‌های نقش و RoleClaims
    public class AuthorizationDbContext
        : IdentityConte<ApplicationRole, Guid>
    {
        public AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options) : base(options) { }

        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("authz");
            builder.ApplyConfiguration(new OutboxMessageConfiguration("authz"));

            // Role
            builder.Entity<ApplicationRole>(b =>
            {
                b.ToTable("AspNetRoles", "authz");
                b.Property(r => r.Description).HasMaxLength(500);
            });

            // RoleClaims
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims", "authz");

            // اگر می‌خواهی رابطه‌ی User↔Role داشته باشی (AspNetUserRoles)،
            // می‌توانی جدول را در authz بسازی. توجه: FK بین اسکیمای authz و auth را یا به صورت غیر-اجرایی نگه دار یا اگر یک دیتابیس است، FK را تنظیم کن.
            builder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles", "authz");
        }
    }

}