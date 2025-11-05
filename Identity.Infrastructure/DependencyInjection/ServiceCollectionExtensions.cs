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
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Identity.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Identity_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(IdentityDbContext).Assembly.GetName().Name;

            services.AddDataProtection();

            // DbContext
            services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__IdentityMigrationsHistory", "identity");
                });
            });

            // Identity (User + Role)
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

            // Outbox registration
            var registration = services.BuildServiceProvider().GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<IdentityDbContext>(services);

            // JWT
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IUnitOfWork<IdentityDbContext>, EfUnitOfWork<IdentityDbContext>>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IRoleResolver, RoleResolver>();
            services.AddHostedService<IdentityModuleInitializer>();

            var jwtSection = configuration.GetSection("Jwt");
            var key = jwtSection["Key"]!;
            var issuer = jwtSection["Issuer"]!;
            var audience = jwtSection["Audience"]!;

            // Authentication + JwtBearer (درست)
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ValidateIssuerSigningKey = true
                    };
                });

            return services;
        }
    }
}