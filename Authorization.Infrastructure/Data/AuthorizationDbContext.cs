using Authorization.Infrastructure.Identity;
using Core.Domain.Common;
using Core.Infrastructure.Database.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Data
{
    public class AuthorizationDbContext : IdentityDbContext<IdentityUser<Guid>, ApplicationRole, Guid>
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

            // جداول مرتبط با Role
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("AspNetRoleClaims", "authz");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("AspNetUserRoles", "authz");
        }
    }

}