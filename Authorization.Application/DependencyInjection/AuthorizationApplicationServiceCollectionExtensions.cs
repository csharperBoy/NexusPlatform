using Authorization.Application.EventHandlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DependencyInjection
{
    public static class AuthorizationApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthorizationApplication(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AssignDefaultRoleEventHandler).Assembly));

            return services;
        }
    }
}