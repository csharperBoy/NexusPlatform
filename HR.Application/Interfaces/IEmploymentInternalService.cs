using Core.Application.Abstractions.HR;
using Core.Domain.Common;
using Core.Domain.ValueObjects;
using HR.Application.DTOs;
using HR.Domain.Entities;
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
        Task<Guid> UpdateEmploymentAsync(
            Guid id,
            Optional<List<string>?> phone,
          Optional<List<string>?> address,
          Optional<List<string>?> email,
          Optional<List<string>?> mobile,
          Optional<string?> firstName,
          Optional<string?> lastName,
          Optional<DateTime?> birthDate,
          Optional<string?> birthPlace,
          Optional<string?> fatherName,
          Optional<string?> nationalCode,
          Optional<string?> employmentCode,
          Optional<Guid?> employmentTypeId,
          Optional<Guid?> employmentStatusId,
          Optional<DateOnly?> startDate,
          Optional<DateOnly?> endDate,
          Optional<List<Guid>?> locationsId,
          Optional<List<string>?> officePhone,
          Optional<List<string>?> orgEmail,
          Optional<List<string>?> orgMobile);
    }
}
