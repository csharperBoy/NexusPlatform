
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.HR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using People.Infrastructure.Data;
using People.Infrastructure.Services;
using People.Application.Interfaces;
namespace People.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection People_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(PersonDbContext).Assembly.GetName().Name;

            // DbContext برای Peopleها
            services.AddDbContext<PersonDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(conn, b =>
                {
                    b.MigrationsAssembly(migrationsAssembly);
                    b.MigrationsHistoryTable("__PeopleMigrationsHistory", "people");
                });
            });
            services.AddScoped<IPersonPublicService>(sp => sp.GetRequiredService<PersonService>());
            services.AddScoped<IPersonInternalService>(sp => sp.GetRequiredService<PersonService>());
            // Resolve از DI
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<PersonDbContext>(services);
            return services;
        }

    }
}
