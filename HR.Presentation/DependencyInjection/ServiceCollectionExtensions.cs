using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Presentation.Controller
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection HR_AddPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
              .AddApplicationPart(typeof(EmployeeController).Assembly)
              .AddControllersAsServices();

            services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(EmployeeController).Assembly));

            return services;
        }
    }
}
