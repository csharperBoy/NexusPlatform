using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Presentation.Controller
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection HR_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
