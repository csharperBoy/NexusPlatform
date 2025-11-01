using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event.Presentation.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Event_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
         return services;
        }

    }
}
