using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Data
{
    public class AuthorizationDbContext : DbContext
    {
        public AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options) : base(options) { }

        // در آینده می‌توانید Permissionها و سایر موجودیت‌های Authorization را اینجا اضافه کنید
        // public DbSet<Permission> Permissions { get; set; }
        // public DbSet<RolePermission> RolePermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("authz"); // Authorization Schema

            // کانفیگوریشن‌های آینده برای Permissionها
        }
    }
}