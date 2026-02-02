using Core.Application.Helper;
using Core.Application.Models;
using Core.Infrastructure.Database;
using Core.Infrastructure.Logging;
using Core.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
namespace Core.Infrastructure.DependencyInjection
{
    /*
     📌 ApplicationBuilderExtensions
     --------------------------------
     این کلاس مجموعه‌ای از **Extension Methods** برای IApplicationBuilder است
     که وظیفه‌ی راه‌اندازی زیرساخت‌های اپلیکیشن (Middlewareها و Swagger) را بر عهده دارد.

     ✅ نکات کلیدی:
     1. Core_UseInfrastructure:
        • ثبت Middlewareهای عمومی برای کل اپلیکیشن.
        • CorrelationIdMiddleware → برای رهگیری درخواست‌ها با شناسه یکتا.
        • ExceptionHandlingMiddleware → برای مدیریت خطاها و بازگرداندن پاسخ استاندارد.

     2. UseSwaggerApplicationBuilderExtensions:
        • فعال‌سازی Swagger و SwaggerUI در همه‌ی محیط‌ها.
        • در محیط Production و Staging:
          - دسترسی به Swagger محدود می‌شود.
          - نیاز به Authentication و Authorization وجود دارد.
          - فقط کاربران با نقش "Admin" مجاز به مشاهده Swagger هستند.
        • در محیط Development → Swagger آزادانه در دسترس است.

     🛠 جریان کار:
     - در زمان راه‌اندازی اپلیکیشن (Program.cs یا Startup.cs):
       app.Core_UseInfrastructure();
       app.UseSwaggerApplicationBuilderExtensions(Configuration);

     - نتیجه:
       • همه‌ی Middlewareهای زیرساختی فعال می‌شوند.
       • Swagger در محیط توسعه آزاد است.
       • در محیط‌های حساس (Production/Staging) دسترسی به Swagger فقط برای Adminها مجاز است.

     📌 نتیجه:
     این کلاس پایه‌ی مکانیزم **Infrastructure Bootstrapping** در معماری ماژولار است
     و تضمین می‌کند که Middlewareها و ابزارهای توسعه (Swagger) به صورت استاندارد و امن راه‌اندازی شوند.
    */

    public static class ApplicationBuilderExtensions
    {
        // 📌 ثبت Middlewareهای زیرساختی
        public static async Task<IApplicationBuilder> Core_UseInfrastructure(this IApplicationBuilder app)
        {

            var moduleSettings = app.ApplicationServices.GetRequiredService<IOptions<ModuleSettings>>().Value;
            ModuleHelper.Initialize(moduleSettings);



            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            return app;
        }

        // 📌 فعال‌سازی Swagger با محدودیت‌های امنیتی در محیط‌های Production/Staging
        public static async Task<IApplicationBuilder> UseSwaggerApplicationBuilderExtensions(
            this IApplicationBuilder app,
            IConfiguration configuration)
        {
            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

            // 📌 فعال‌سازی Swagger در همه‌ی محیط‌ها
            app.UseSwagger();
            app.UseSwaggerUI();

            // 📌 محدودسازی دسترسی به Swagger در Production و Staging
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

                            // 📌 فقط نقش Admin مجاز است
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
