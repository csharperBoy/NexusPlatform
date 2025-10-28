using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Presentation.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Audit_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
