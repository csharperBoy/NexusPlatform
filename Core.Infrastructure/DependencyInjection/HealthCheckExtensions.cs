using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.DependencyInjection
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection Core_AddHealthChecks(this IServiceCollection services, IConfiguration config)
        {
           
            return services;
        }
    }
}
