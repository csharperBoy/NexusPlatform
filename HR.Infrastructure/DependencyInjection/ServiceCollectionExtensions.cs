using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.Identity;
using Core.Infrastructure.Repositories;
using HR.Infrastructure.Data;
using HR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HR.Application.Interfaces;
using HR.Domain.Entities;

namespace HR.Infrastructure.DependencyInjection
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
        public static IServiceCollection HR_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // 📌 گرفتن Connection String از تنظیمات
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(HRDbContext).Assembly.GetName().Name;

            // 📌 رجیستر DbContext برای ماژول Sample
            services.AddDbContext<HRDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    // تعیین Assembly محل Migrationها
                    b.MigrationsAssembly(migrationsAssembly);

                    // تعیین جدول تاریخچه Migrationها در اسکیمای "sample"
                    b.MigrationsHistoryTable("__HRHistory", "hr");
                });
            });

            services.AddScoped<OrgChartService>();
            services.AddScoped<EmployeeService>();
            services.AddScoped<IUnitOfWork<HRDbContext>, EfUnitOfWork<HRDbContext>>();
            // 📌 رجیستر Repository مبتنی بر Specification
            //services.AddScoped<ISpecificationRepository<SampleEntity, Guid>, EfSpecificationRepository<SampleDbContext, SampleEntity, Guid>>();
            
            services.AddScoped<IOrgChartPublicService>(sp => sp.GetRequiredService<OrgChartService>());
            services.AddScoped<IOrgChartInternalService>(sp => sp.GetRequiredService<OrgChartService>());

            services.AddScoped<IEmployeePublicService>(sp => sp.GetRequiredService<EmployeeService>());
            services.AddScoped<IEmployeeInternalService>(sp => sp.GetRequiredService<EmployeeService>());

            services.AddScoped<IRepository<HRDbContext, Employment, Guid>, EfRepository<HRDbContext, Employment, Guid>>();
            services.AddScoped<ISpecificationRepository<Employment, Guid>, EfSpecificationRepository<HRDbContext, Employment, Guid>>();

            services.AddScoped<IRepository<HRDbContext, Assignment, Guid>, EfRepository<HRDbContext, Assignment, Guid>>();
            services.AddScoped<ISpecificationRepository<Assignment, Guid>, EfSpecificationRepository<HRDbContext, Assignment, Guid>>();

            services.AddScoped<IRepository<HRDbContext, CostCenter, Guid>, EfRepository<HRDbContext, CostCenter, Guid>>();
            services.AddScoped<ISpecificationRepository<CostCenter, Guid>, EfSpecificationRepository<HRDbContext, CostCenter, Guid>>();

            services.AddScoped<IRepository<HRDbContext, EmploymentStatus, Guid>, EfRepository<HRDbContext, EmploymentStatus, Guid>>();
            services.AddScoped<ISpecificationRepository<EmploymentStatus, Guid>, EfSpecificationRepository<HRDbContext, EmploymentStatus, Guid>>();

            services.AddScoped<IRepository<HRDbContext, EmploymentType, Guid>, EfRepository<HRDbContext, EmploymentType, Guid>>();
            services.AddScoped<ISpecificationRepository<EmploymentType, Guid>, EfSpecificationRepository<HRDbContext, EmploymentType, Guid>>();

            services.AddScoped<IRepository<HRDbContext, Grade, Guid>, EfRepository<HRDbContext, Grade, Guid>>();
            services.AddScoped<ISpecificationRepository<Grade, Guid>, EfSpecificationRepository<HRDbContext, Grade, Guid>>();

            services.AddScoped<IRepository<HRDbContext, JobLevel, Guid>, EfRepository<HRDbContext, JobLevel, Guid>>();
            services.AddScoped<ISpecificationRepository<JobLevel, Guid>, EfSpecificationRepository<HRDbContext, JobLevel, Guid>>();

            services.AddScoped<IRepository<HRDbContext, JobTitle, Guid>, EfRepository<HRDbContext, JobTitle, Guid>>();
            services.AddScoped<ISpecificationRepository<JobTitle, Guid>, EfSpecificationRepository<HRDbContext, JobTitle, Guid>>();

            services.AddScoped<IRepository<HRDbContext, OrganizationUnit, Guid>, EfRepository<HRDbContext, OrganizationUnit, Guid>>();
            services.AddScoped<ISpecificationRepository<OrganizationUnit, Guid>, EfSpecificationRepository<HRDbContext, OrganizationUnit, Guid>>();

            services.AddScoped<IRepository<HRDbContext, Post, Guid>, EfRepository<HRDbContext, Post, Guid>>();
            services.AddScoped<ISpecificationRepository<Post, Guid>, EfSpecificationRepository<HRDbContext, Post, Guid>>();

            // 📌 رجیستر HostedService برای مقداردهی اولیه ماژول
            services.AddHostedService<ModuleInitializer>();

            services.AddScoped<IOrgChartInternalService, OrgChartService>();
            services.AddScoped<IEmployeeInternalService, EmployeeService>();
            // 📌 رجیستر OutboxProcessor برای پردازش رویدادهای دامنه
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<HRDbContext>(services);

            return services;
        }
    }
}

