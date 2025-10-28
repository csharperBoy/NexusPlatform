using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using User.Infrastructure.Data;


namespace User.Infrastructure.DependencyInjection
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection Audit_AddHealthChecks(this IServiceCollection services, IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection");

            services.AddHealthChecks()
                    .AddDbContextCheck<UserManagementDbContext>("UserDatabase");


            return services;
        }
    }
}
