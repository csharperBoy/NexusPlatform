using Navigation.Domain.Entities;
using Navigation.Infrastructure.Configurations;
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

namespace Navigation.Infrastructure.Data
{

    public class NavigationDbContext : Base_DbContext
    {
        public NavigationDbContext(
              DbContextOptions<NavigationDbContext> options,
              IServiceProvider serviceProvider)
              : base(options, serviceProvider)
        {
        }
        public NavigationDbContext(DbContextOptions<NavigationDbContext> options)
      : base(options, new ServiceCollection().BuildServiceProvider())
        {
        }
        // 📌 جدول مربوط به موجودیت BaseEntity
        public DbSet<Menu> Menu { get; set; } = null!;

        // 📌 جدول مربوط به OutboxMessage برای ذخیره رویدادهای دامنه
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 📌 تعیین اسکیمای پیش‌فرض برای جداول این ماژول
            modelBuilder.HasDefaultSchema("navigation");

            // 📌 اعمال تنظیمات OutboxMessageConfiguration
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("navigation"));

            modelBuilder.ApplyConfiguration(new MenuConfiguration());

        }
    }

}
