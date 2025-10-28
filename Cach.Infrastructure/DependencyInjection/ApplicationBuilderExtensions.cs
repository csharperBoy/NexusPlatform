using Audit.Infrastructure.Data;
using Core.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cach.Infrastructure.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task<IApplicationBuilder> Cach_UseInfrastructure(this IApplicationBuilder app)
        {
            return app;
        }

   }
}
