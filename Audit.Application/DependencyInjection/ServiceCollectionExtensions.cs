using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Audit_AddApplication(this IServiceCollection services, IConfiguration configuration)
        { 
            // رجیستر MediatR و هندلرها
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
            });
            return services;
        }
    }
}