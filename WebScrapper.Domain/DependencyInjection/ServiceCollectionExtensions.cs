using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebScrapper.Domain.DependencyInjection
{
   
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection WebScrapper_AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}

