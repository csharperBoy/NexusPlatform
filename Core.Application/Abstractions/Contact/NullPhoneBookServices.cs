using Core.Application.Abstractions.People;
using Core.Domain.ValueObjects;
using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
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
    public class NullHrContactPublicServices : IContactPublicService
    {
        public Task CreateContact(ContactTypeEnum type, List<string>? value, Guid profileId)
        {
            throw new NotImplementedException();
        }

        public Task<Guid> CreateContactProfileAsync(string Title, ContactProfileTypeEnum Type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        

        public Task<List<ContactItemDto>> GetContactsByProfilesIdsAsync(List<Guid> profilesId)
        {
            throw new NotImplementedException();
        }

      
        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }

        public Task<bool> SyncProfileContacts(ContactTypeEnum type, List<string>? values, Guid profileId)
        {
            throw new NotImplementedException();
        }
    }
    
}
