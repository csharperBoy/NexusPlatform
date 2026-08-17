using Core.Application.Abstractions.People;
using Core.Domain.ValueObjects;
using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Contact
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection PhoneBook_NullServiceInject(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddScoped<IPhoneBookPublicService, NullPhoneBookServices>();
            return services;
        }
    }
    public class NullPhoneBookServices : IPhoneBookPublicService
    {
       
    }
    public class NullHrContactPublicServices : IHrContactPublicService
    {
        public async Task CreateEmploymentContact(HrContactType type, string? value, Guid employmentId)
        {
            await Task.CompletedTask;
        }

        public async Task CreateLocationContact(HrContactType type, string? value, Guid LocationId)
        {
            await Task.CompletedTask;
        }

        public async Task CreatePostContact(HrContactType type, string? value, Guid postId)
        {
            await Task.CompletedTask;
        }

        public Task<List<EntityContactDto<HrContactType>>> GetEmploymentContactsByEmploymentIdsAsync(List<Guid> employmentIds)
        {
            throw new NotImplementedException();
        }

        public  Task<List<EntityContactDto<HrContactType>>> GetLocationContactsByLocationIdsAsync(List<Guid> locationIds)
        {
            return null;
        }

        public Task<List<EntityContactDto<HrContactType>>> GetPostContactsByPostIdsAsync(List<Guid> postIds)
        {
            throw new NotImplementedException();
        }

        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }
    }
    public class NullPeopleContactPublicServices : IPeopleContactPublicService
    {
        public async Task CreatePartyContact(PartyContactType type, string? value, Guid partyId)
        {
            await Task.CompletedTask;
        }

        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }
    }
}
