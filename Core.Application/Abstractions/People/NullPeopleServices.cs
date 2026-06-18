using Core.Application.Abstractions.HR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.People
{
   
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection People_NullServiceInject(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IPersonPublicService, NullPersonService>();
            return services;
        }
    }
    public class NullPersonService : IPersonPublicService
    {
    }
 
}
