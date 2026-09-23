using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scheduler.Application.Abstractions;

namespace Scheduler.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Scheduler_AddApplication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // رجیستر MediatR و همه Handlerهای موجود در اسمبلی Application
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

            return services;
        }

        /// <summary>
        /// ثبت یک handler برای payload مشخص.
        /// - handler رو توی DI ثبت می‌کنه
        /// - payload type رو به Registry (توی Infrastructure) معرفی می‌کنه
        /// مصرف‌کننده‌ها (مثل Trader) این متد رو توی ServiceCollectionExtensions خودشون صدا می‌زنن.
        /// </summary>
        public static IServiceCollection AddScheduledJobHandler<TPayload, THandler>(
            this IServiceCollection services)
            where TPayload : class, IScheduledJobPayload
            where THandler : class, IScheduledJobHandler<TPayload>
        {
            services.AddScoped<IScheduledJobHandler<TPayload>, THandler>();
            services.AddSingleton(new ScheduledJobPayloadRegistration(typeof(TPayload)));
            return services;
        }
    }

    /// <summary>
    /// Marker برای ثبت payload type.
    /// Infrastructure موقع ساخت JobTypeRegistry این registrationها رو می‌خونه.
    /// </summary>
    public record ScheduledJobPayloadRegistration(Type PayloadType);
}