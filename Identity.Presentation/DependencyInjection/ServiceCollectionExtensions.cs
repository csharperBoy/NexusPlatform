using Identity.Presentation.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Presentation.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection Identity_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(AuthController).Assembly)
                .AddControllersAsServices();

            services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AuthController).Assembly));

            return services;
        }
    }
}
