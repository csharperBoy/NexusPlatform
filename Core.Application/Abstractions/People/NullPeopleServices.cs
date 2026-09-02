using Core.Application.Abstractions.HR;
using Core.Domain.Common;
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
        PhoneNumber? Mobile = null , string? createBy = null)
        {
            return Task.FromResult<Guid>(Guid.Empty);
        }

        public Task<Guid> CreatePersonAsync(string nationalCode, string firstName, string lastName, DateTime? birthDate = null, string? birthPlace = null, string? fatherName = null, Gender? gender = null, List<PhoneNumber>? Phone = null, List<string>? Address = null, List<Email>? Email = null, List<PhoneNumber>? Mobile = null, string? createBy = null)
        {
            throw new NotImplementedException();
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

        public async Task UpdatePersonAsync(Guid id, string? phone, string? address, string? email, string? mobile, string firstlName, string lastName, DateTime? birthDate, string? birthPlace, string? fatherName, string? nationalCode)
        {

            await Task.CompletedTask;
        }

        public Task UpdatePersonAsync(Guid id, string firstlName, string lastName, DateTime? birthDate, string? birthPlace, string? fatherName, string? nationalCode, List<PhoneNumber>? Phone = null, List<string>? Address = null, List<Email>? Email = null, List<PhoneNumber>? Mobile = null)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePersonAsync(Guid id, Optional<string> firstlName, Optional<string> lastName, Optional<DateTime?> birthDate, Optional<string?> birthPlace, Optional<string?> fatherName, Optional<string?> nationalCode, Optional<List<PhoneNumber>?> Phone, Optional<List<string>?> Address, Optional<List<Email>?> Email, Optional<List<PhoneNumber>?> Mobile)
        {
            throw new NotImplementedException();
        }

        Task<bool> IPersonPublicService.UpdatePersonAsync(Guid id, Optional<string> firstlName, Optional<string> lastName, Optional<DateTime?> birthDate, Optional<string?> birthPlace, Optional<string?> fatherName, Optional<string?> nationalCode, Optional<List<PhoneNumber>?> Phone, Optional<List<string>?> Address, Optional<List<Email>?> Email, Optional<List<PhoneNumber>?> Mobile)
        {
            throw new NotImplementedException();
        }
    }

}
