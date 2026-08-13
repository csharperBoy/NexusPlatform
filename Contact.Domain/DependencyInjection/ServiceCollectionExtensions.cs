using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contact.Domain.DependencyInjection
{
   
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Contact_AddDomain(this IServiceCollection services, IConfiguration configuration)
        {
           return services;
        }
    }
}

