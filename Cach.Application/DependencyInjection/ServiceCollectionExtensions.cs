
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Cach.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Cach_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
         
            return services;
        }
    }
}