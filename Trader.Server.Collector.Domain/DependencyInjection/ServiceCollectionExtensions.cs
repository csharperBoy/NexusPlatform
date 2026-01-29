using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Trader.Server.Collector.Domain.DependencyInjection
{
 
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TraderServer_AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
           return services;
        }
    }
}

