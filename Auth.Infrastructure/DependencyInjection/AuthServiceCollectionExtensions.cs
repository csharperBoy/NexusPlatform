using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Auth.Infrastructure.Data;
using Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Auth.Infrastructure.Services;
using Auth.Application;

namespace Auth.Infrastructure.DependencyInjection
{
    public static class AuthServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<AuthDbContext>(options =>
                options.UseSqlServer(conn, b => b.MigrationsAssembly("Auth.Infrastructure")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredLength = 6;
                // تنظیمات دیگر را اینجا قرار دهید
            })
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

            // token service
            services.AddScoped<ITokenService, JwtTokenService>();

            // auth service
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}
