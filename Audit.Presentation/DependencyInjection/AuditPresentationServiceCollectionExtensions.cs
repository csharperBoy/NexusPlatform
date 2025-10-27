using Audit.Presentation.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Presentation.DependencyInjection
{
    public static class AuditPresentationServiceCollectionExtensions
    {
        public static IServiceCollection AddAuditPresentation(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(AuditLogsController).Assembly)
                .AddControllersAsServices();

            services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AuditLogsController).Assembly));

            return services;
        }
    }
}
