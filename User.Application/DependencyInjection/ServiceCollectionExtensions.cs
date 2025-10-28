using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace User.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection User_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}