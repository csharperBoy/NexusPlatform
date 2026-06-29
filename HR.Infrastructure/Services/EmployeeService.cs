using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
using Core.Shared.Enums.HR;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Specifications;
using HR.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Infrastructure.Services
{
    public class EmployeeService : IEmployeeInternalService, IEmployeePublicService
    {
        private readonly IRepository<HRDbContext, Employment, Guid> _employeeRepository;
        private readonly IRepository<HRDbContext, EmploymentLocation, Guid> _employeeLocationsRepository;
        private readonly ISpecificationRepository<Employment, Guid> _employeeSpecRepository;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;

        public EmployeeService(IRepository<HRDbContext, Employment, Guid> employeeRepository, ILogger<EmployeeService> logger,
            ISpecificationRepository<Employment, Guid> employeeSpecRepository, IRepository<HRDbContext, EmploymentLocation, Guid> employeeLocationsRepository,
            IUnitOfWork<HRDbContext> uow)
        {
            _employeeRepository = employeeRepository;
            _employeeLocationsRepository = employeeLocationsRepository;
            _employeeSpecRepository = employeeSpecRepository;
            _logger = logger;
            _uow = uow;
        }

        public async Task<Guid> CreateEmployeeAsync(
             string _EmployeeCode,
         Guid _PersonId,
         Guid _EmploymentTypeId,
         Guid _EmploymentStatusId,
         DateOnly _StartDate,
         DateOnly? _EndDate
            )
        {
            Employment person = new Employment(_EmployeeCode, _PersonId, _EmploymentTypeId, _EmploymentStatusId, _StartDate, _EndDate);
            await _employeeRepository.AddAsync(person);
            return person.Id;
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }
        public async Task<Guid?> GetEmployeeId(Guid? personId)
        {
            GetEmployeeByPersonIdSpec spec = new GetEmployeeByPersonIdSpec(personId);
            Employment? employee = await _employeeSpecRepository.GetBySpecAsync(spec);
            if (employee == null) 
                throw new InvalidOperationException("employee not found!!!");

            return employee.Id;

        }

        public async Task AssignLocationsToEmployee(Guid employeeId, List<Guid> locationsId)
        {
            List<EmploymentLocation> lst = locationsId.Select(l => new EmploymentLocation(l, employeeId)).ToList();
            await _employeeLocationsRepository.AddRangeAsync(lst);
        }
    }
}
