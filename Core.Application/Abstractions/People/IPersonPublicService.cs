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
             PhoneNumber? Phone = null,
        string? Address = null,
        Email? Email = null,
        PhoneNumber? Mobile = null);
        Task<Guid?> GetPersonPermissionAssigneeIdAsync(Guid? personId);
        Task<Guid?> GetPartyPermissionAssigneeIdAsync(Guid? partyId);
        Task SaveAsync();
        Task<Guid?> GetNaturalPersonIdAsync(Guid? partyId);
    }
}
