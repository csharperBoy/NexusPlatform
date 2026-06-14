

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace People.Presentation.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection User_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
