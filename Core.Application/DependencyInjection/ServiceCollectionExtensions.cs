using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.Identity;
using Core.Application.Abstractions.Security;
using Core.Application.Behaviors;
using Core.Application.Models;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Core.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection Core_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            

            services.Core_AllModuleNullServiceInject(configuration);
            // اضافه کردن Pipeline Behaviors برای همه Requestها
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AccessBehavior<,>));

            return services;
        }
        public static IServiceCollection Core_AllModuleNullServiceInject(this IServiceCollection services, IConfiguration configuration)
        {

            services.HR_NullServiceInject(configuration);
            services.Identity_NullServiceInject(configuration);

            return services;
        }
    }
}