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
        List<PhoneNumber>? Mobile = null , string? createBy = null);
        Task<Guid?> GetPersonPermissionAssigneeIdAsync(Guid? personId);
        Task<Guid?> GetPartyPermissionAssigneeIdAsync(Guid? partyId);
        Task SaveAsync();
        Task<Guid?> GetNaturalPersonIdAsync(Guid? partyId);
        Task UpdatePersonAsync(Guid id, string firstlName, string lastName, DateTime? birthDate, string? birthPlace, string? fatherName, string? nationalCode,

             List<PhoneNumber>? Phone = null,
       List<string>? Address = null,
        List<Email>? Email = null,
        List<PhoneNumber>? Mobile = null );
    }
}
