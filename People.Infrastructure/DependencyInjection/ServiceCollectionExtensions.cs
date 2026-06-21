
using Core.Application.Abstractions;
using Core.Application.Abstractions.Events;
using Core.Application.Abstractions.People;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using People.Application.Interfaces;
using People.Domain.Entities;
using People.Infrastructure.Data;
using People.Infrastructure.Services;
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

            services.AddScoped<PersonService>();

            services.AddScoped<IUnitOfWork<PeopleDbContext>, EfUnitOfWork<PeopleDbContext>>();

            services.AddScoped<IPersonPublicService>(sp => sp.GetRequiredService<PersonService>());
            services.AddScoped<IPersonInternalService>(sp => sp.GetRequiredService<PersonService>());

            services.AddScoped<IRepository<PeopleDbContext, legalPersons, Guid>, EfRepository<PeopleDbContext, legalPersons, Guid>>();
            services.AddScoped<ISpecificationRepository<legalPersons, Guid>, EfSpecificationRepository<PeopleDbContext, legalPersons, Guid>>();

            services.AddScoped<IRepository<PeopleDbContext, naturalPersons, Guid>, EfRepository<PeopleDbContext, naturalPersons, Guid>>();
            services.AddScoped<ISpecificationRepository<naturalPersons, Guid>, EfSpecificationRepository<PeopleDbContext, naturalPersons, Guid>>();

            services.AddScoped<IRepository<PeopleDbContext, Parties, Guid>, EfRepository<PeopleDbContext, Parties, Guid>>();
            services.AddScoped<ISpecificationRepository<Parties, Guid>, EfSpecificationRepository<PeopleDbContext, Parties, Guid>>();

            services.AddScoped<IRepository<PeopleDbContext, PersonContact, Guid>, EfRepository<PeopleDbContext, PersonContact, Guid>>();
            services.AddScoped<ISpecificationRepository<PersonContact, Guid>, EfSpecificationRepository<PeopleDbContext, PersonContact, Guid>>();

            services.AddScoped<IRepository<PeopleDbContext, PersonProfile, Guid>, EfRepository<PeopleDbContext, PersonProfile, Guid>>();
            services.AddScoped<ISpecificationRepository<PersonProfile, Guid>, EfSpecificationRepository<PeopleDbContext, PersonProfile, Guid>>();

            // Resolve از DI
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<PeopleDbContext>(services);
            return services;
        }

    }
}
