// Core/Infrastructure/DependencyInjection/DatabaseServiceCollectionExtensions.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Core.Infrastructure.Database;

namespace Core.Infrastructure.DependencyInjection
{
    public static class DatabaseServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IMigrationManager, MigrationManager>();
            return services;
        }
    }
}
