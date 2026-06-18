
using Core.Application.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using People.Infrastructure.Data;
using People.Infrastructure.Services;
using People.Application.Interfaces;
using Core.Application.Abstractions.People;
namespace People.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection People_AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(PeopleDbContext).Assembly.GetName().Name;

            // DbContext برای Peopleها
            services.AddDbContext<PeopleDbContext>((serviceProvider, options) =>
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
            registration.AddOutboxProcessor<PeopleDbContext>(services);
            return services;
        }

    }
}
