using Core.Application.Abstractions.HR;
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
        Guid _EmploymentTypeId,
        Guid _EmploymentStatusId,
        DateOnly _StartDate,
        DateOnly? _EndDate
           );
    }
}
