using Core.Application.Abstractions.Security;
using Core.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Data
{
    public abstract class BaseDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        protected BaseDbContext(DbContextOptions options, IServiceProvider serviceProvider)
            : base(options)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            UpdateAuditableEntities();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateAuditableEntities();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            UpdateAuditableEntities();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void UpdateAuditableEntities()
        {
            var entries = ChangeTracker
                .Entries<AuditableEntity>()
                .ToList();

            if (!entries.Any()) return;

            // دریافت ICurrentUserService به صورت lazy
            var currentUserService = _serviceProvider.GetService<ICurrentUserService>();
            var currentUserId = GetCurrentUserId(currentUserService);
            var currentUserName = GetCurrentUserName(currentUserService);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUserId;

                    // برای Modified هم در حالت Added مقدار دهی می‌کنیم
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = currentUserId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = currentUserId;

                    // از تغییر CreatedAt جلوگیری می‌کنیم
                    entry.Property(nameof(AuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(AuditableEntity.CreatedBy)).IsModified = false;
                }
            }
        }

        private string GetCurrentUserId(ICurrentUserService? currentUserService)
        {
            // اولویت با ICurrentUserService است
            if (currentUserService?.UserId != null)
            {
                return currentUserService.UserId.ToString()!;
            }

            // اگر ICurrentUserService نبود، از HttpContext استفاده می‌کنیم
            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            var userId = httpContextAccessor?.HttpContext?.User?.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId))
            {
                return userId;
            }

            // اگر هیچکدام نبود، از Environment
            return Environment.UserName ?? "system";
        }

        private string GetCurrentUserName(ICurrentUserService? currentUserService)
        {
            if (!string.IsNullOrEmpty(currentUserService?.UserName))
            {
                return currentUserService.UserName;
            }

            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            var userName = httpContextAccessor?.HttpContext?.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(userName))
            {
                return userName;
            }

            return Environment.UserName ?? "system";
        }
    }
}