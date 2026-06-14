using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using People.Infrastructure.Data;

namespace People.Infrastructure.DependencyInjection
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection People_AddHealthChecks(this IServiceCollection services, IConfiguration config)
        {
            var conn = config.GetConnectionString("DefaultConnection");

            services.AddHealthChecks()
                    .AddDbContextCheck<PersonDbContext>("PeopleDatabase");


            return services;
        }
    }
}
