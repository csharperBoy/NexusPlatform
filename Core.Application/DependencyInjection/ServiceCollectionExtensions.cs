using Core.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Core.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
     
        public static IServiceCollection Core_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
         
            // اضافه کردن Pipeline Behaviors برای همه Requestها
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));

            return services;
        }
    }
}