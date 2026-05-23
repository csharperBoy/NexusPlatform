using Authorization.Application.Interfaces;
using Authorization.Application.Interfaces.Processor;
using Authorization.Application.Interfaces.Service;
using Authorization.Application.Provider;
using Authorization.Domain.Entities;
using Authorization.Infrastructure.Data;
using Authorization.Infrastructure.HostedServices;
using Authorization.Infrastructure.Processor;
using Authorization.Infrastructure.Services;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.Processor;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Events;
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
//using IAuthorizationService = Authorization.Application.Interfaces.IAuthorizationService;

namespace Authorization.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Authorization_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(AuthorizationDbContext).Assembly.GetName().Name;

            services.AddDataProtection();

            services.AddDbContext<AuthorizationDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__AuthorizationMigrationsHistory", "authorization");
                });
            });

            services.AddScoped<ResourceService>();
            services.AddScoped<PermissionService>();
            services.AddScoped<ScopeService>();

            services.AddScoped<IResourceInternalService>(sp => sp.GetRequiredService<ResourceService>());
            services.AddScoped<IPermissionInternalService>(sp => sp.GetRequiredService<PermissionService>());
            services.AddScoped<IScopeInternalService>(sp => sp.GetRequiredService<ScopeService>());

            services.AddScoped<IResourcePublicService>(sp => sp.GetRequiredService<ResourceService>());
            services.AddScoped<IPermissionPublicService>(sp => sp.GetRequiredService<PermissionService>());
            services.AddScoped<IScopePublicService>(sp => sp.GetRequiredService<ScopeService>());


            services.AddTransient(typeof(IRowLevelSecurityProcessor<>), typeof(RowLevelSecurityProcessor<>));
            services.AddTransient<IResourceProcessor, ResourceProcessor>();
            services.AddTransient<IScopeProcessor, ScopeProcessor>();
            services.AddTransient<IAuthorizationProcessor, AuthorizationProcessor>();

            services.AddScoped<IRepository<AuthorizationDbContext, Resource, Guid>, EfRepository<AuthorizationDbContext, Resource, Guid>>();
            services.AddScoped<IRepository<AuthorizationDbContext, Permission, Guid>, EfRepository<AuthorizationDbContext, Permission, Guid>>();
            services.AddScoped<IRepository<AuthorizationDbContext, PermissionRule, Guid>, EfRepository<AuthorizationDbContext, PermissionRule, Guid>>();
            services.AddScoped<IRepository<AuthorizationDbContext, Scope, Guid>, EfRepository<AuthorizationDbContext, Scope, Guid>>();

            services.AddScoped<ISpecificationRepository<Resource, Guid>, EfSpecificationRepository<AuthorizationDbContext, Resource, Guid>>();
            services.AddScoped<ISpecificationRepository<Permission, Guid>, EfSpecificationRepository<AuthorizationDbContext, Permission, Guid>>();
            services.AddScoped<ISpecificationRepository<PermissionRule, Guid>, EfSpecificationRepository<AuthorizationDbContext, PermissionRule, Guid>>();
            services.AddScoped<ISpecificationRepository<Scope, Guid>, EfSpecificationRepository<AuthorizationDbContext, Scope, Guid>>();


            services.AddScoped<IUnitOfWork<AuthorizationDbContext>, EfUnitOfWork<AuthorizationDbContext>>();
            var registration = services.BuildServiceProvider().GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<AuthorizationDbContext>(services);

            services.AddHostedService<ModuleInitializer>();
            services.AddScoped<IPermissionInternalService, PermissionService>();
            services.AddScoped<IResourceInternalService, ResourceService>();
           
            return services;
        }
        public static IServiceCollection Authorization_AddInfrastructure_InApp(this IServiceCollection services, IConfiguration configuration)
        {
            #region test


            // 2. گرفتن لیست DbContextهای ثبت‌شده
            var dbContextTypes = services
                .Where(sd => typeof(DbContext).IsAssignableFrom(sd.ServiceType) && !sd.ServiceType.IsAbstract)
                .Select(sd => sd.ServiceType)
                .ToList();

            // 3. ثبت provider
            services.AddSingleton<IResourceMetadataProvider>(sp =>
            {
                return new ResourceMetadataProvider(sp, dbContextTypes);
            });


            #endregion
            return services;
        }
    }
}