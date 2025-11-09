using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Domain.Entities;
using Sample.Infrastructure.Data;

namespace Sample.Infrastructure.DependencyInjection
{
    /*
     📌 ServiceCollectionExtensions (Infrastructure Layer)
     -----------------------------------------------------
     این کلاس یک Extension برای IServiceCollection است که وظیفه‌اش رجیستر کردن سرویس‌های
     مربوط به ماژول Sample در DI Container می‌باشد.

     ✅ نکات کلیدی:
     - متد اصلی: Sample_AddInfrastructure
       → در زمان راه‌اندازی برنامه (Startup/Program.cs) فراخوانی می‌شود.
       → سرویس‌های زیر رجیستر می‌شوند:
         1. SampleDbContext → اتصال به دیتابیس ماژول Sample با SQL Server.
            - تعیین Assembly برای Migrationها.
            - تعیین جدول تاریخچه Migrationها با نام سفارشی "__SampleMigrationsHistory" در اسکیمای "sample".
         2. ISpecificationRepository<SampleEntity, Guid> → رجیستر Repository مبتنی بر Specification.
         3. ModuleInitializer → HostedService برای مقداردهی اولیه ماژول (Seed Data).
         4. OutboxProcessor → رجیستر پردازشگر Outbox برای انتشار رویدادهای دامنه.

     🛠 جریان کار:
     1. در Program.cs متد Sample_AddInfrastructure فراخوانی می‌شود.
     2. DbContext و Repositoryها به DI اضافه می‌شوند.
     3. HostedService (ModuleInitializer) فعال می‌شود تا داده‌های اولیه درج شوند.
     4. OutboxProcessor رجیستر می‌شود تا رویدادهای دامنه ذخیره‌شده در Outbox پردازش شوند.
     5. در نهایت سرویس‌ها آماده‌ی استفاده در کل برنامه هستند.

     📌 نتیجه:
     این کلاس نقطه‌ی ورودی ماژول Sample به DI Container است و تضمین می‌کند که
     همه‌ی سرویس‌های زیرساختی (DbContext، Repository، Outbox، HostedService) به صورت استاندارد رجیستر شوند.
    */

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Sample_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(SampleDbContext).Assembly.GetName().Name;

            // 📌 رجیستر DbContext برای ماژول Sample
            services.AddDbContext<SampleDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    // تعیین Assembly محل Migrationها
                    b.MigrationsAssembly(migrationsAssembly);

                    // تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
                    b.MigrationsHistoryTable("__SampleMigrationsHistory", "sample");
                });
            });

            // 📌 رجیستر Repository مبتنی بر Specification
            services.AddScoped<ISpecificationRepository<SampleEntity, Guid>, EfSpecificationRepository<SampleDbContext, SampleEntity, Guid>>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<SampleDbContext>(services);

            return services;
        }
    }
}

