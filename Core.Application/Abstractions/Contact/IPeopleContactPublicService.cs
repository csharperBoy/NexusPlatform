using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.People;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Contact
{
    public interface IPeopleContactPublicService
    {
        Task<List<EntityContactDto<PartyContactType>>> GetPartyContactsByPartyIdsAsync(List<Guid> partyIds);
        Task CreatePartyContact(PartyContactType type, string? value, Guid partyId);
        Task SaveAsync();
        
    }
}
