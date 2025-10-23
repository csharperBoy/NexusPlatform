using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Cors; // اضافه کردن این using
using Core.Application.Models;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching;
using Core.Application.Abstractions.Events;
using Core.Infrastructure.Caching;
using Core.Infrastructure.Events;
using Core.Infrastructure.HealthChecks;
using Core.Infrastructure.Repositories;
using MediatR;
using Core.Application.Behaviors;

namespace Core.Infrastructure.DependencyInjection
{
    public static class CoreInfrastructureServiceCollectionExtensions
    {

        public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // ثبت تنظیمات - روش صحیح
            services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
            services.Configure<CorsSettings>(configuration.GetSection("Cors"));
            services.Configure<HealthCheckSettings>(configuration.GetSection("HealthCheck"));

            // سرویس‌های پایه
            services.AddHttpContextAccessor();

            // ثبت ساده و مستقیم
            services.AddScoped(typeof(IRepository<,,>), typeof(EfRepository<,,>));

            services.AddScoped(typeof(ISpecificationRepository<,>), typeof(EfSpecificationRepository<,>));

            // Event Bus
            services.AddScoped<IEventBus, MediatorEventBus>();

            // Health Checks
            services.AddScoped<IHealthCheckService, HealthCheckService>();
            // Database Services
            services.AddDatabaseServices(configuration);

            // Logging Services
            services.AddLoggingServices(configuration);

            services.AddOutboxServices();

            // پیکربندی کش
            services.AddCachingServices(configuration);

            // پیکربندی CORS
            ConfigureCors(services, configuration);

            // Validation Pipeline
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

            return services;
        }

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
    }
}
