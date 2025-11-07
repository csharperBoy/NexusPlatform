using Core.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Sample.Application.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Sample_AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMediatR(cfg =>
               cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
            // Pipeline behaviors
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RetryBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuditBehavior<,>));
            return services;
        }
    }
}