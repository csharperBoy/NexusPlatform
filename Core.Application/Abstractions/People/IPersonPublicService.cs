using Core.Domain.Common;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.People
{
    public interface IPersonPublicService
    {
        Task<Guid> CreatePersonAsync(string nationalCode, string firstName, string lastName,
            DateTime? birthDate = null,
            string? birthPlace = null,
            string? fatherName = null,
            Gender? gender = null,
             List<PhoneNumber>? Phone = null,
        List<string>? Address = null,
        List<Email>? Email = null,
        List<PhoneNumber>? Mobile = null, string? createBy = null);
        Task<Guid?> GetPersonPermissionAssigneeIdAsync(Guid? personId);
        Task<Guid?> GetPartyPermissionAssigneeIdAsync(Guid? partyId);
        Task SaveAsync();
        Task<Guid?> GetNaturalPersonIdAsync(Guid? partyId);
        Task<bool> UpdatePersonAsync(Guid id,
            Optional<string> firstlName,
            Optional<string> lastName,
            Optional<DateTime?> birthDate,
            Optional<string?> birthPlace,
            Optional<string?> fatherName,
            Optional<string?> nationalCode,
            Optional<List<PhoneNumber>?> Phone,
            Optional<List<string>?> Address ,
            Optional<List<Email>?> Email,
            Optional<List<PhoneNumber>?> Mobile );
    }
}
