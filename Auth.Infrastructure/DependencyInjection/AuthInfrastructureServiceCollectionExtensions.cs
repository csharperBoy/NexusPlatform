using Auth.Application;
using Auth.Infrastructure.Configuration;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Identity;
using Auth.Infrastructure.Services;
using Core.Application.Abstractions;
using Core.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Auth.Infrastructure.DependencyInjection
{
    public static class AuthInfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(AuthDbContext).Assembly.GetName().Name;

            // DbContext
            services.AddDbContext<AuthDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__AuthMigrationsHistory", "auth");
                });
            });

            // Identity (with ApplicationRole and Guid keys)
            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // Password policy
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false; // یا true برحسب نیاز
                // Lockout
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                // User
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

            // Token settings via options pattern
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            services.AddScoped<IUnitOfWork, EfUnitOfWork<AuthDbContext>>();

            // Register other infra services (to implement next)
            //services.AddScoped<ISessionManager, SessionManager>();
            //services.AddScoped<IPasswordPolicyService, PasswordPolicyService>();
            //services.AddScoped<IIpRestrictionService, IpRestrictionService>();
            //services.AddScoped<IAuditEventPublisher, AuditEventPublisher>();

            // Authentication: JWT Bearer
            var jwtSection = configuration.GetSection("Jwt");
            var key = jwtSection["Key"];
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];

            services.AddAuthentication(options =>
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
                    ValidateIssuerSigningKey = true
                };
            });

            services.AddAuthorization();

            // Hosted service for migration + seed
            services.AddHostedService<AuthModuleInitializer>();

            return services;
        }
    }
}
