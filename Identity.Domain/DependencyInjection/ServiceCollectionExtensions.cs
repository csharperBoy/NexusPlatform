using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Domain.DependencyInjection
{
   

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Identity_AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}

