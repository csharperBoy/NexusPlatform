using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Contact
{
    public interface IContactPublicService
    {
        Task<Guid> CreateContactProfileAsync(string Title, ContactProfileTypeEnum Type, CancellationToken cancellationToken = default);
        //Task CreateEmploymentContact(ContactTypeEnum type, List<string>? value, Guid employmentId);
        //Task CreateLocationContact(ContactTypeEnum type, List<string>? value, Guid LocationId);
        //Task CreatePostContact(ContactTypeEnum type, List<string>? value, Guid postId);
        //Task CreateContact(ContactTypeEnum type, List<string>? value, Guid profileId);
        Task SyncProfileContacts(ContactTypeEnum type, List<string>? values, Guid profileId);
        Task<List<ContactItemDto>> GetContactsByProfilesIdsAsync(List<Guid> profilesId);
        //Task<List<ContactItemDto>> GetLocationContactsByLocationIdsAsync(List<Guid> locationIds);
        //Task<List<ContactItemDto>> GetPostContactsByPostIdsAsync(List<Guid> postIds);
        //Task<List<ContactItemDto>> GetEmploymentContactsByEmploymentIdsAsync(List<Guid> employmentIds);

        Task SaveAsync();
       
    }
}
