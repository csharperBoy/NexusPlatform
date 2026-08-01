using Core.Application.Abstractions.HR;
using Core.Domain.ValueObjects;
using HR.Domain.Entities;
using HR.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Interfaces
{
    public interface IEmployeeInternalService : IEmployeePublicService
    {
        Task AssignLocationsToEmployee(Guid employeeId, List<Guid> locationsId);
        Task<Guid> CreateEmployeeAsync(
            string _EmployeeCode,
        Guid _PersonId,
        Guid? _EmploymentTypeId,
        Guid? _EmploymentStatusId,
        DateOnly? _StartDate = null,
        DateOnly? _EndDate = null,

        PhoneNumber? _orgPhone = null,
        Email? _orgEmail = null,
        PhoneNumber? _orgMobile = null
           );
        Task<IReadOnlyList<EmployementInfoView>> GetEmploymentListAsync();
        Task<Guid> UpdateEmploymentAsync(Guid id, string? phone, string? address, string? email, string? mobile, string firstlName, string lastName, DateTime? birthDate, string? birthPlace, string? fatherName, string employeeCode, Guid? employmentTypeId, Guid? employmentStatusId, DateOnly? startDate, DateOnly? endDate, List<Guid> locationsId, string? officePhone, string? orgEmail, string? orgMobile);
    }
}
