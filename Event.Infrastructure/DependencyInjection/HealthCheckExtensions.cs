using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Infrastructure.DependencyInjection
{
    public static class HealthCheckExtensions
    {
        public static IServiceCollection Event_AddHealthChecks(this IServiceCollection services, IConfiguration config)
        {
           
            return services;
        }
    }
}
