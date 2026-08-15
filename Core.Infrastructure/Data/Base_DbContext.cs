using Core.Application.Context;
using Core.Application.Helper;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Core.Infrastructure.Data
{
    public interface IBase_DbContext
    {
        void EnsureTrigger(string RootNamespace, string fileName, string triggerName, Assembly? assembly = null);
        void EnsureView(string RootNamespace, string fileName, string viewName, string schema = "dbo", Assembly? assembly = null);

        void EnsureTriggers(CancellationToken cancellationToken = default(CancellationToken));
        void EnsureViews(CancellationToken cancellationToken = default(CancellationToken));
    }
    public abstract class Base_DbContext : DbContext, IBase_DbContext
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
        public virtual void EnsureViews(CancellationToken cancellationToken = default(CancellationToken))
        {

        }
        public void EnsureTrigger(string RootNamespace, string fileName, string triggerName, Assembly? assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();
            var sqlScript = EmbeddedSqlHelper.Read(RootNamespace, fileName, assembly);

            // بررسی وجود تریگر و ایجاد آن در صورت نبود
            var checkTriggerSql = @"
                                        IF NOT EXISTS (SELECT 1 FROM sys.triggers WHERE name = '" + triggerName + @"' AND parent_class = 1)
                                        BEGIN
                                            EXEC sp_executesql N'" + sqlScript.Replace("'", "''") + @"'
                                        END
                                    ";

            Database.ExecuteSqlRaw(checkTriggerSql);
        }
        /*
        public void EnsureView(string RootNamespace, string fileName, string viewName, string schema = "dbo", Assembly? assembly = null)
        {
            
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();

            var sqlScript = EmbeddedSqlHelper.Read(RootNamespace, fileName, assembly);

            // جایگزینی اسکیما در کل اسکریپت
            sqlScript = sqlScript.Replace("[dbo]", $"[{schema}]");

            // استفاده از @p0 و @p1 برای پارامترها
            var checkViewSql = @"
                                    IF NOT EXISTS (
                                        SELECT 1 
                                        FROM sys.views v
                                        INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
                                        WHERE s.name = @p0 AND v.name = @p1
                                    )
                                    BEGIN
                                        EXEC sp_executesql N'" + sqlScript.Replace("'", "''") + @"'
                                    END
                                ";

            Database.ExecuteSqlRaw(checkViewSql, schema, viewName);
        }
       */
        public void EnsureView(string RootNamespace, string fileName, string viewName, string schema = "dbo", Assembly? assembly = null)
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<Base_DbContext>>();
            try
            {
                logger.LogInformation("Starting EnsureView for ViewName={ViewName}, Schema={Schema}, FileName={FileName}", viewName, schema, fileName);

                if (assembly == null)
                {
                    assembly = Assembly.GetCallingAssembly();
                    logger.LogDebug("Assembly resolved to {AssemblyName}", assembly.FullName);
                }

                // 1. خواندن اسکریپت از فایل امبد
                string sqlScript;
                try
                {
                    sqlScript = EmbeddedSqlHelper.Read(RootNamespace, fileName, assembly);
                    logger.LogInformation("Embedded script loaded. Length={Length}", sqlScript.Length);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to read embedded script. RootNamespace={RootNamespace}, FileName={FileName}", RootNamespace, fileName);
                    throw;
                }

                // 2. جایگزینی اسکیما
                string originalScript = sqlScript;
                sqlScript = sqlScript.Replace("[dbo]", $"[{schema}]");
                if (originalScript != sqlScript)
                    logger.LogDebug("Schema replacement performed. New script starts with: {Preview}", sqlScript.Substring(0, Math.Min(200, sqlScript.Length)));
                else
                    logger.LogWarning("No [dbo] found in script to replace with [{Schema}]", schema);

                // 3. ساخت اسکریپت بررسی و اجرای شرطی
                // برای جلوگیری از مشکلات نقلقول، کل اسکریپت را به عنوان پارامتر به sp_executesql میدهیم
                // و از پارامترهای @p0 و @p1 برای schema و viewName استفاده میکنیم.
                var checkViewSql = @"
            IF NOT EXISTS (
                SELECT 1 
                FROM sys.views v
                INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
                WHERE s.name = @p0 AND v.name = @p1
            )
            BEGIN
                EXEC sp_executesql @script
            END
        ";

                // پارامتر @script را به صورت جداگانه تعریف میکنیم
                var parameters = new object[]
                {
            new SqlParameter("@p0", schema),
            new SqlParameter("@p1", viewName),
            new SqlParameter("@script", sqlScript) // کل اسکریپت را به عنوان پارامتر NVARCHAR ارسال میکنیم
                };

                // لاگ اسکریپت نهایی (فقط برای دیباگ، ممکن است طولانی باشد)
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("CheckView SQL: {CheckSql}", checkViewSql);
                    logger.LogDebug("Script parameter (first 500 chars): {ScriptPreview}", sqlScript.Substring(0, Math.Min(500, sqlScript.Length)));
                }

                // 4. اجرا
                try
                {
                    Database.ExecuteSqlRaw(checkViewSql, parameters);
                    logger.LogInformation("EnsureView completed successfully for ViewName={ViewName}", viewName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "SQL execution failed. ViewName={ViewName}, Schema={Schema}", viewName, schema);
                    // لاگ دقیقتر: پارامترها
                    logger.LogError("Parameters: p0={p0}, p1={p1}, ScriptLength={Length}", schema, viewName, sqlScript.Length);
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in EnsureView for ViewName={ViewName}", viewName);
                throw;
            }
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