using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Core.Infrastructure.Repositories;
using Core.Application.Abstractions;

namespace Core.Infrastructure.DependencyInjection
{
    public static class CoreInfrastructureServiceCollectionExtensions
    {
        
        public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services)
        {

            services.AddHttpContextAccessor();
            services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
            return services;
        }

    }
}
