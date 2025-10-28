
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Authorization.Presentation.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Authorization_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            
            return services;
        }
    }
}
