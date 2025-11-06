using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Sample.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Sample_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}