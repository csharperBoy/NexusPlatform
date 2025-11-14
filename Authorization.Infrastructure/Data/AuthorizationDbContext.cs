using Authorization.Domain.Entities;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Data
{
    public class AuthorizationDbContext : DbContext
    {
        public AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<UserPermission> UserPermissions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasDefaultSchema("authorization");

            // apply configurations manually or by scanning assembly
            builder.ApplyConfiguration(new Configurations.ResourceConfiguration());
            builder.ApplyConfiguration(new Configurations.PermissionConfiguration());
            builder.ApplyConfiguration(new Configurations.RolePermissionConfiguration());
            builder.ApplyConfiguration(new Configurations.UserPermissionConfiguration());
        }
    }

}
