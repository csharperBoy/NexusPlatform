
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

            services.AddScoped<IRepository<PeopleDbContext, LegalPerson, Guid>, EfRepository<PeopleDbContext, LegalPerson, Guid>>();
            services.AddScoped<ISpecificationRepository<LegalPerson, Guid>, EfSpecificationRepository<PeopleDbContext, LegalPerson, Guid>>();

            services.AddScoped<IRepository<PeopleDbContext, NaturalPerson, Guid>, EfRepository<PeopleDbContext, NaturalPerson, Guid>>();
            services.AddScoped<ISpecificationRepository<NaturalPerson, Guid>, EfSpecificationRepository<PeopleDbContext, NaturalPerson, Guid>>();

            services.AddScoped<IRepository<PeopleDbContext, Party, Guid>, EfRepository<PeopleDbContext, Party, Guid>>();
            services.AddScoped<ISpecificationRepository<Party, Guid>, EfSpecificationRepository<PeopleDbContext, Party, Guid>>();

            services.AddScoped<IRepository<PeopleDbContext, PartyContact, Guid>, EfRepository<PeopleDbContext, PartyContact, Guid>>();
            services.AddScoped<ISpecificationRepository<PartyContact, Guid>, EfSpecificationRepository<PeopleDbContext, PartyContact, Guid>>();

            services.AddScoped<IRepository<PeopleDbContext, NaturalPersonProfile, Guid>, EfRepository<PeopleDbContext, NaturalPersonProfile, Guid>>();
            services.AddScoped<ISpecificationRepository<NaturalPersonProfile, Guid>, EfSpecificationRepository<PeopleDbContext, NaturalPersonProfile, Guid>>();

            // Resolve از DI
            var registration = services.BuildServiceProvider()
                                       .GetRequiredService<IOutboxProcessorRegistration>();
            registration.AddOutboxProcessor<PeopleDbContext>(services);
            return services;
        }

    }
}
