using Authorization.Application.Interfaces;
using Authorization.Infrastructure.Authorization;
using Authorization.Infrastructure.HostedServices;
using Authorization.Infrastructure.Services;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.Security;
using Core.Infrastructure.DependencyInjection;
using Core.Infrastructure.Repositories;
using Identity.Application;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Infrastructure.Configuration;
using Identity.Infrastructure.Data;
using Identity.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
            // Outbox registration
            var registration = services.BuildServiceProvider().GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<AuthorizationDbContext>(services);

            services.AddHostedService<ModuleInitializer>();

            services.AddScoped<IPermissionService, PermissionService>();
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // discover IPermissionDefinitionProvider via DI (modules register their providers)
            services.AddHostedService<PermissionRegistrarHostedService>();

            return services;
        }
    }
}