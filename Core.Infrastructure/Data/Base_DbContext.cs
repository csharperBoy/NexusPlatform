using Core.Application.Context;
using Core.Application.Helper;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Infrastructure.Data
{
    public interface IBase_DbContext 
    {
        void EnsureTrigger(string RootNamespace, string fileName, string triggerName, Assembly? assembly = null);
        void EnsureTriggers(CancellationToken cancellationToken = default(CancellationToken));
    }
    public abstract class Base_DbContext : DbContext , IBase_DbContext
    {
        private readonly IServiceProvider _serviceProvider;

         protected Base_DbContext(DbContextOptions options, IServiceProvider serviceProvider)
             : base(options)
         {
             _serviceProvider = serviceProvider;
         }
        public virtual void EnsureTriggers(CancellationToken cancellationToken = default(CancellationToken))
        {

        }
         public void EnsureTrigger(string RootNamespace, string fileName , string triggerName , Assembly? assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();
            var sqlScript = EmbeddedSqlHelper.Read(RootNamespace, fileName , assembly);

            // بررسی وجود تریگر و ایجاد آن در صورت نبود
            var checkTriggerSql = @"
                                        IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE name = '" + triggerName + @"' AND parent_class = 1)
                                        BEGIN
                                            EXEC sp_executesql N'" + sqlScript.Replace("'", "''") + @"'
                                        END
                                    ";

            Database.ExecuteSqlRaw(checkTriggerSql);
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