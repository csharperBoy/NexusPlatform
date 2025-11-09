using Core.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Sample.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /*
       📌 ServiceCollectionExtensions (Application Layer)
       -------------------------------------------------
       این کلاس برای رجیستر کردن سرویس‌های مربوط به Application Layer در DI (Dependency Injection) استفاده می‌شود.

       ✅ نکات کلیدی:
       - متد Sample_AddApplication در Startup فراخوانی می‌شود تا همه قابلیت‌های Application رجیستر شوند.
       - MediatR رجیستر می‌شود تا Commandها، Queryها و Eventها به Handlerهای مربوطه متصل شوند.
       - Pipeline Behaviors اضافه می‌شوند تا قبل و بعد از اجرای هر Request منطق مشترک اجرا شود.

       🛠 Behaviors:
       - LoggingBehavior: لاگ گرفتن از Request و Response.
       - ValidationBehavior: اعتبارسنجی ورودی‌ها با FluentValidation.
       - RetryBehavior: اجرای مجدد عملیات در صورت خطا (با استفاده از Polly).
       - AuditBehavior: ثبت لاگ‌های مربوط به Audit (چه کسی چه کاری انجام داد).

       📌 نتیجه:
       این فایل نقطه‌ی ورود Application Layer به DI است و نشان می‌دهد چطور باید MediatR و Behaviors را رجیستر کنیم.
      */
        public static IServiceCollection Sample_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            // رجیستر MediatR و همه Handlerهای موجود در اسمبلی Application
            services.AddMediatR(cfg =>
               cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

            // اضافه کردن Pipeline Behaviors برای همه Requestها
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
            return services;
        }
    }
}