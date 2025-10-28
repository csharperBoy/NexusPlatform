using Core.Infrastructure.Database;
using Core.Infrastructure.Logging;
using Core.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task<IApplicationBuilder> Core_UseInfrastructure(this IApplicationBuilder app)
        {

            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            return app;
        }

    }
}
