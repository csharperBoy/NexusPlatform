using Core.Infrastructure.Database;
using Core.Infrastructure.Logging;
using Core.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Event.Infrastructure.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task<IApplicationBuilder> Event_UseInfrastructure(this IApplicationBuilder app)
        {


            return app;
        }

      
    }
}
