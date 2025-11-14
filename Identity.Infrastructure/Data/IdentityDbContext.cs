using Core.Domain.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Identity.Domain.Entities;
using Core.Infrastructure.Database.Configurations;
using Identity.Infrastructure.Configuration;

namespace Identity.Infrastructure.Data
{
    public class IdentityDbContext
           : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
                               IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
                               IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
            : base(options)
        {
        }

        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<UserSession> UserSessions { get; set; } = null!;

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // اگر DomainEvents داری اینجا می‌شه dispatch کرد
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Schema
            builder.HasDefaultSchema("identity");

            // Outbox
            builder.ApplyConfiguration(new OutboxMessageConfiguration("identity"));

            // Apply all entity configurations
            builder.ApplyConfiguration(new ApplicationUserConfiguration());
            builder.ApplyConfiguration(new ApplicationRoleConfiguration());
            builder.ApplyConfiguration(new RefreshTokenConfiguration());
            builder.ApplyConfiguration(new UserSessionConfiguration());

            // Identity default table names
            builder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles", "identity");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("AspNetUserClaims", "identity");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("AspNetUserLogins", "identity");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims", "identity");
            builder.Entity<IdentityUserToken<Guid>>().ToTable("AspNetUserTokens", "identity");
        }
    }
}
