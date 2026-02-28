using Core.Application.Abstractions.Security;
using Core.Application.Context;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
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
                .Entries<IAuditableEntity>()
                .ToList();

            if (!entries.Any()) return;

            // دریافت ICurrentUserService به صورت lazy
            var currentUserContext = _serviceProvider.GetService<UserDataContext>();
            var currentUserId = currentUserContext.UserId;
            var currentUserName = currentUserContext.UserName;

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUserId.ToString();

                    // برای Modified هم در حالت Added مقدار دهی می‌کنیم
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = currentUserId.ToString();
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = currentUserId.ToString();

                    // از تغییر CreatedAt جلوگیری می‌کنیم
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                }
            }
        }

        
    }
}