using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Data;
using Authorization.Infrastructure.HostedServices;
using Authorization.Infrastructure.Processor;
using Authorization.Infrastructure.Services;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Application.Behaviors;
using Core.Infrastructure.DependencyInjection;
using Core.Infrastructure.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IAuthorizationService = Authorization.Application.Interfaces.IAuthorizationService;

namespace Authorization.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Authorization_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(AuthorizationDbContext).Assembly.GetName().Name;

            services.AddDataProtection();

            // DbContext
            services.AddDbContext<AuthorizationDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__AuthorizationMigrationsHistory", "authorization");
                });
            });

            // 1. ثبت خودِ سرویس (Implementation)
            services.AddScoped<ResourceService>();
            services.AddScoped<PermissionService>();
            // 2. ثبت اینترفیس داخلی (برای استفاده داخل ماژول)
            // ارجاع می‌دهیم به همان Instance بالایی
            services.AddScoped<IResourceInternalService>(sp => sp.GetRequiredService<ResourceService>());
            services.AddScoped<IPermissionInternalService>(sp => sp.GetRequiredService<PermissionService>());
            // 3. ثبت اینترفیس عمومی (برای استفاده بقیه ماژول‌ها)
            // این هم ارجاع می‌شود به همان Instance
            services.AddScoped<IResourcePublicService>(sp => sp.GetRequiredService<ResourceService>());
            services.AddScoped<IPermissionPublicService>(sp => sp.GetRequiredService<PermissionService>());


            services.AddTransient(typeof(IAuthorizationProcessor<>), typeof(AuthorizationProcessor<>));
            services.AddScoped<IDataScopeProcessor, DataScopeProcessor>();
            //services.AddScoped<IDataScopeService, DataScopeService>();

            // سرویس‌های دیگری که احتمالاً نیاز دارید و باید چک کنید ثبت شده باشند:
            services.AddScoped<IAuthorizationChecker, AuthorizationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();
            services.AddScoped<IDataScopeEvaluator, DataScopeEvaluator>();

            services.AddScoped<IRepository<AuthorizationDbContext, Resource, Guid>, EfRepository<AuthorizationDbContext, Resource, Guid>>();
            services.AddScoped<IRepository<AuthorizationDbContext,Permission, Guid>, EfRepository<AuthorizationDbContext, Permission, Guid>>();
           
            services.AddScoped<ISpecificationRepository<Resource, Guid>, EfSpecificationRepository<AuthorizationDbContext, Resource, Guid>>();
            services.AddScoped<ISpecificationRepository<Permission, Guid>, EfSpecificationRepository<AuthorizationDbContext, Permission, Guid>>();
            

            services.AddScoped<IUnitOfWork<AuthorizationDbContext>, EfUnitOfWork<AuthorizationDbContext>>();
            // Outbox registration
            var registration = services.BuildServiceProvider().GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<AuthorizationDbContext>(services);

            services.AddHostedService<ModuleInitializer>();

            services.AddScoped<IDataScopeEvaluator, DataScopeEvaluator>();
            services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();
            services.AddScoped<IPermissionInternalService, PermissionService>();
            services.AddScoped<IResourceInternalService, ResourceService>();
            services.AddScoped<IResourceTreeBuilder, ResourceTreeBuilder>();

            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IAuthorizationChecker, AuthorizationService>();
            
            // Resource Definition Providers
            //services.AddSingleton<AuthorizationResourceDefinitionProvider>();
            //services.AddSingleton<IResourceDefinitionProvider>(sp =>
                //sp.GetRequiredService<AuthorizationResourceDefinitionProvider>());
            // discover IPermissionDefinitionProvider via DI (modules register their providers)
            //services.AddHostedService<ResourceRegistrarHostedService>();
            return services;
        }
    }
}