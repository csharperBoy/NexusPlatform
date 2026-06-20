using Core.Application.Abstractions.HR;
using Core.Shared.Enums.HR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.People
{
   
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection People_NullServiceInject(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IPersonPublicService, NullPersonService>();
            return services;
        }
    }
    public class NullPersonService : IPersonPublicService
    {
        public Task<Guid> CreatePersonAsync(string nationalCode, string firstName, string lastName, DateTime? birthDate, string? birthPlace, string? fatherName, Gender? gender)
        {
            return Task.FromResult<Guid>(Guid.Empty);
        }

        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }
    }

}
