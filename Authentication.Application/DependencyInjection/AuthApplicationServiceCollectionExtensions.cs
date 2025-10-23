using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authentication.Application.DependencyInjection
{
    public static class AuthApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // رجیستر MediatR و هندلرها
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AuthApplicationServiceCollectionExtensions).Assembly);
            });

            return services;
        }
    }
}
