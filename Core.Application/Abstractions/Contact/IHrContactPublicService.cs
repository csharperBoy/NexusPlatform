using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Contact
{
    public interface IHrContactPublicService
    {
        Task CreateEmploymentContact(HrContactType type, string? value, Guid employmentId);
        Task CreateLocationContact(HrContactType type, string? value, Guid LocationId);
        Task<List<EntityContactDto<HrContactType>>> GetLocationContactsByLocationIdsAsync(List<Guid> locationIds);
        Task<List<EntityContactDto<HrContactType>>> GetPostContactsByPostIdsAsync(List<Guid> postIds);
        Task<List<EntityContactDto<HrContactType>>> GetEmploymentContactsByEmploymentIdsAsync(List<Guid> employmentIds);
        
        Task CreatePostContact(HrContactType type, string? value, Guid postId);
        Task SaveAsync();
       
    }
}
