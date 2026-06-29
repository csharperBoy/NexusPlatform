
using Core.Application.Helper;
using Core.Domain.Common;
using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Infrastructure.Data
{

    public class BaseDbContext : Base_DbContext
    {
        public BaseDbContext(
              DbContextOptions<BaseDbContext> options,
              IServiceProvider serviceProvider)
              : base(options, serviceProvider)
        {
        }
        public BaseDbContext(DbContextOptions<BaseDbContext> options)
      : base(options, new ServiceCollection().BuildServiceProvider())
        {
        }
      
        // 📌 جدول مربوط به OutboxMessage برای ذخیره رویدادهای دامنه
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 📌 تعیین اسکیمای پیش‌فرض برای جداول این ماژول
            modelBuilder.HasDefaultSchema("base");

            // 📌 اعمال تنظیمات OutboxMessageConfiguration
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("base"));


        }
    }

}
