using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Sample.Domain.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Sample_AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
