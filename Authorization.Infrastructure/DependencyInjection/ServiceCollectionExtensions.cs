using Authorization.Application.Interfaces;
using Authorization.Infrastructure.Data;
using Authorization.Infrastructure.Identity;
using Authorization.Infrastructure.Services;
using Core.Application.Abstractions.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Authorization_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(AuthorizationDbContext).Assembly.GetName().Name;

            // DbContext برای Roleها
            services.AddDbContext<AuthorizationDbContext>(options =>
            {
                options.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__AuthzMigrationsHistory", "authz");
                });
            });

            // Identity فقط برای Role
            services.AddIdentityCore<IdentityUser<Guid>>() // فقط برای سازگاری
                .AddRoles<ApplicationRole>()
                .AddEntityFrameworkStores<AuthorizationDbContext>()
                .AddDefaultTokenProviders();

            // سرویس‌های Authorization
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IRoleResolver, RoleResolver>();

            return services;
        }
    }
}
