using Core.Application.Abstractions.HR;
using Core.Domain.ValueObjects;
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
        public Task<Guid> CreatePersonAsync(string nationalCode, string firstName, string lastName,
            DateTime? birthDate = null,
            string? birthPlace = null,
            string? fatherName = null,
            Gender? gender = null,
             PhoneNumber? Phone = null,
        string? Address = null,
        Email Email = null,
        PhoneNumber? Mobile = null)
        {
            return Task.FromResult<Guid>(Guid.Empty);
        }

        public Task<Guid?> GetNaturalPersonIdAsync(Guid? partyId)
        {
            return null;
        }

        public Task<Guid?> GetPartyPermissionAssigneeIdAsync(Guid? partyId)
        {
            return null;
        }

        public Task<Guid?> GetPersonPermissionAssigneeIdAsync(Guid? personId)
        {
            return null;
        }

        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }
    }

}
