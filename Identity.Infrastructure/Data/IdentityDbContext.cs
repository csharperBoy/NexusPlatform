using Core.Application.Helper;
using Core.Domain.Common;
using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using Identity.Domain.Entities;
using Identity.Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Identity.Infrastructure.Data
{
    public class IdentityDbContext
           : IdentityDbContext<ApplicationUser, ApplicationRole, Guid,
                               IdentityUserClaim<Guid>, IdentityUserRole<Guid>, IdentityUserLogin<Guid>,
                               IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
        ,IBase_DbContext
    {
        public virtual void EnsureTriggers(CancellationToken cancellationToken = default(CancellationToken))
        {

        }
        public void EnsureTrigger(string RootNamespace, string fileName, string triggerName, Assembly? assembly = null)
        {
            var sqlScript = EmbeddedSqlHelper.Read(RootNamespace, fileName);

            // بررسی وجود تریگر و ایجاد آن در صورت نبود
            var checkTriggerSql = @"
                                        IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE name = '" + triggerName + @"' AND parent_class = 1)
                                        BEGIN
                                            EXEC sp_executesql N'" + sqlScript.Replace("'", "''") + @"'
                                        END
                                    ";

            Database.ExecuteSqlRaw(checkTriggerSql);
        }

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
