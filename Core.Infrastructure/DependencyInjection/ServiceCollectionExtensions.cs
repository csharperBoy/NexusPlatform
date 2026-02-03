using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Application.Behaviors;
using Core.Application.Models;
using Core.Infrastructure.Database;
using Core.Infrastructure.HealthChecks;
using Core.Infrastructure.Logging;
using Core.Infrastructure.Repositories;
using Core.Infrastructure.Resilience;
using Core.Infrastructure.Security;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors; // اضافه کردن این using
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using System.ComponentModel.Design;
namespace Core.Infrastructure.DependencyInjection
{
    /*
     📌 ServiceCollectionExtensions
     ------------------------------
     این کلاس مجموعه‌ای از **Extension Methods** برای IServiceCollection است
     که وظیفه‌ی ثبت سرویس‌های زیرساختی (Infrastructure Services) در DI Container را بر عهده دارد.

     ✅ نکات کلیدی:
     - Core_AddInfrastructure:
       • نقطه‌ی ورودی برای ثبت همه‌ی سرویس‌های زیرساختی.
       • شامل موارد زیر:
         1. Swagger → برای مستندسازی و تست API.
         2. CorsSettings / HealthCheckSettings → تنظیمات امنیتی و مانیتورینگ.
         3. HttpContextAccessor → دسترسی به HttpContext در سرویس‌ها.
         4. CurrentUserService → سرویس برای شناسایی کاربر جاری.
         5. MigrationManager → مدیریت Migrationهای دیتابیس.
         6. Repositoryها:
            - IRepository<,,> → پیاده‌سازی عمومی با EF Core.
            - ISpecificationRepository<,> → پیاده‌سازی با Specification Pattern.
         7. LoggingServices → پیکربندی Serilog برای لاگ‌گذاری.
         8. Cors → پیکربندی سیاست‌های CORS بر اساس تنظیمات.
         9. ValidationBehavior → Pipeline Behavior برای اعتبارسنجی درخواست‌ها (MediatR).
         10. ResiliencePolicies → سیاست‌های تحمل خطا (Resilience).
         11. MediatR → ثبت همه‌ی Handlerها از Assemblies جاری.

     - ConfigureCors:
       • خواندن تنظیمات CORS از IConfiguration.
       • تعریف سیاست پیش‌فرض با Origins مجاز.
       • اجازه‌ی Headerها، Methodها و Credentials.

     - AddLoggingServices:
       • پیکربندی Serilog به عنوان Logger اصلی.
       • پاک‌سازی Providerهای پیش‌فرض و جایگزینی با Serilog.
       • استفاده از تنظیمات موجود در IConfiguration.

     🛠 جریان کار:
     1. در زمان راه‌اندازی اپلیکیشن (Program.cs یا Startup.cs)، این متد فراخوانی می‌شود:
        services.Core_AddInfrastructure(Configuration);
     2. همه‌ی سرویس‌های زیرساختی در DI ثبت می‌شوند.
     3. سایر لایه‌ها (Application, Domain, Presentation) می‌توانند این سرویس‌ها را استفاده کنند.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Infrastructure Bootstrapping** در معماری ماژولار است
     و تضمین می‌کند که سرویس‌های زیرساختی به صورت استاندارد و یکپارچه در DI Container ثبت شوند.
    */

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Core_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            #region ثبت ماژول های فعال در کلاس helper

            services.Configure<ModuleSettings>(
                configuration.GetSection("Modules"));

            // گزینه ۲: ثبت به صورت Singleton برای دسترسی مستقیم
            services.AddSingleton(provider =>
            {
                // روش صحیح: ابتدا ConfigurationSection را بگیرید
                var section = configuration.GetSection("Modules");
                var settings = new ModuleSettings();

                // Bind کردن تنظیمات
                section.Bind(settings);
                return settings;
            });

            #endregion

            // 📌 ثبت سرویس‌های زیرساختی
            services.AddSwaggerGen();
            services.Configure<CorsSettings>(configuration.GetSection("Cors"));
            services.Configure<HealthCheckSettings>(configuration.GetSection("HealthCheck"));

            services.AddHttpContextAccessor();

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            services.AddScoped<IMigrationManager, MigrationManager>();
            services.AddScoped(typeof(IRepository<,,>), typeof(EfRepository<,,>));
            //services.AddScoped(typeof(ISpecificationRepository<,>), typeof(EfSpecificationRepository<,,>));

            services.AddLoggingServices(configuration);
            ConfigureCors(services, configuration);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddResiliencePolicies(configuration);
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

            return services;
        }

        // 📌 پیکربندی CORS
        private static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
        {
            var corsSettings = configuration.GetSection("Cors").Get<CorsSettings>();
            var allowedOrigins = corsSettings?.AllowedOrigins ?? new[] { "http://localhost:3000" };

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });
        }

        // 📌 پیکربندی Logging با Serilog
        private static IServiceCollection AddLoggingServices(this IServiceCollection services, IConfiguration configuration)
        {
            Log.Logger = SerilogConfiguration.CreateConfiguration(configuration).CreateLogger();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });

            return services;
        }

        public static IServiceCollection AddScopedWithAsync<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, Task<TService>> implementationFactory) where TService : class
        {
            return services.AddScoped(provider =>
                implementationFactory(provider).GetAwaiter().GetResult());
        }
    }
}
