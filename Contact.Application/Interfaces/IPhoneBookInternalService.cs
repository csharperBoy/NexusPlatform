using Contact.Application.DTOs;
using Core.Application.Abstractions.Contact;
using Core.Application.Abstractions.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Interfaces
{
    public interface IPhoneBookInternalService : IPhoneBookPublicService
    {
        Task<IReadOnlyList<PhoneBookEmploymentDto>> GetPhoneBookListAsync(Guid? organUnitId);
    }
}
