using Auth.Infrastructure.Identity;
using Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Auth.Infrastructure.Data
{
    public class AuthDbContext : IdentityDbContext<ApplicationUser>, IUnitOfWork
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        // پیاده‌سازی IUnitOfWork
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // جدا کردن شِمای دیتابیس برای Auth
            builder.HasDefaultSchema("auth");

            // اگر نیاز به کانفیگ Fluent دارید همین‌جا اضافه کنید
        }
    }
}
