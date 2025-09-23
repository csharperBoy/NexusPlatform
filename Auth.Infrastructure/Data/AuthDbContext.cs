using Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Data
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // تنظیم schema اختصاصی برای ماژول Auth
            builder.HasDefaultSchema("auth");

            // اگر نیاز به پیکربندی خاص دارین، اینجا اضافه کنید
            // builder.Entity<ApplicationUser>().ToTable("Users");
        }
    }
}
