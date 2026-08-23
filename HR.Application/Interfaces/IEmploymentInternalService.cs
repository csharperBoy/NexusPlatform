using Core.Application.Abstractions.HR;
using Core.Domain.ValueObjects;
using HR.Application.DTOs;
using HR.Domain.Entities;
using HR.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Interfaces
{
    public interface IEmploymentInternalService : IEmploymentPublicService
    {
        Task AssignLocationsToEmployment(Guid employmentId, List<Guid> locationsId);
        Task<Guid> CreateEmploymentAsync(
            string _EmploymentCode,
        Guid _PersonId,
        Guid? _EmploymentTypeId,
        Guid? _EmploymentStatusId,
        DateOnly? _StartDate = null,
        DateOnly? _EndDate = null,

        List<PhoneNumber>? _orgPhone = null,
        List<Email>? _orgEmail = null,
        List<PhoneNumber>? _orgMobile = null
           );
        Task DeleteAsync(Guid id);
        Task<IReadOnlyList<EmploymentInfoDto>> GetEmploymentListAsync();
        Task<Guid> UpdateEmploymentAsync(Guid id, List<string>? phone, List<string>? address, List<string>? email, List<string>? mobile, string? firstlName, string? lastName, DateTime? birthDate, string? birthPlace, string? fatherName, string? nationalCode, string? employmentCode, Guid? employmentTypeId, Guid? employmentStatusId, DateOnly? startDate, DateOnly? endDate, List<Guid>? locationsId, List<string>? officePhone, List<string>? orgEmail, List<string>? orgMobile);
    }
}
