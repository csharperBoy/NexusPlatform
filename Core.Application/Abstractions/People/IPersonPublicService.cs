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
        Task<Guid> CreatePersonAsync(string nationalCode, string firstName, string lastName, DateTime? birthDate, string? birthPlace, string? fatherName, Gender? gender);
        Task SaveAsync();
    }
}
