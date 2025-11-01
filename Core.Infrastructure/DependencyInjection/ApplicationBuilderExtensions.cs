using Core.Infrastructure.Database;
using Core.Infrastructure.Logging;
using Core.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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

        public static async Task<IApplicationBuilder> UseSwaggerApplicationBuilderExtensions(
        this IApplicationBuilder app,
        IConfiguration configuration)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

            // فعال‌سازی Swagger در همه‌ی محیط‌ها
            app.UseSwagger();
            app.UseSwaggerUI();

            // محدودسازی دسترسی به Swagger در Production و Staging
            if (env.IsProduction() || env.IsStaging())
            {
                app.MapWhen(ctx =>
                    ctx.Request.Path.StartsWithSegments("/swagger") ||
                    ctx.Request.Path.StartsWithSegments("/swagger/index.html") ||
                    ctx.Request.Path.StartsWithSegments("/swagger/v1/swagger.json"),
                    builder =>
                    {
                        builder.UseAuthentication();
                        builder.UseAuthorization();

                        builder.Use(async (context, next) =>
                        {
                            if (!context.User.Identity?.IsAuthenticated ?? true)
                            {
                                context.Response.StatusCode = 401;
                                await context.Response.WriteAsync("Unauthorized");
                                return;
                            }

                            // اختیاری: فقط نقش خاص
                            if (!context.User.IsInRole("Admin"))
                            {
                                context.Response.StatusCode = 403;
                                await context.Response.WriteAsync("Forbidden");
                                return;
                            }

                            await next();
                        });

                        builder.UseSwagger();
                        builder.UseSwaggerUI();
                    });
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            return app;
        }

    }
}
