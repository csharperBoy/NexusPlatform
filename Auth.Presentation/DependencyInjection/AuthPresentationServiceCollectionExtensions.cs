using Auth.Presentation.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Presentation.DependencyInjection
{
    public static class AuthPresentationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuthPresentation(this IServiceCollection services, IConfiguration configuration)
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
