using Identity.Application.EventHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Auth_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // رجیستر MediatR و هندلرها
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            });
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssignDefaultRoleEventHandler).Assembly));

            return services;
        }
    }
}