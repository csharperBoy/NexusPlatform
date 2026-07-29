using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.PhoneBook;
using PhoneBook.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoneBook.Application.Interfaces
{
    public interface IPhoneBookInternalService : IPhoneBookPublicService
    {
        Task<IReadOnlyList<PhoneBookEmployeeDto>> GetPhoneBookListAsync(Guid? organUnitId);
    }
}
