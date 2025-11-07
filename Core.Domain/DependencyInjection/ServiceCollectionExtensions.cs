using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Core.Domain.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Core_AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
