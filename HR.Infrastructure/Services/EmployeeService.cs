using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Specifications;
using HR.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HR.Infrastructure.Services
{
    public class EmployeeService : IEmployeeInternalService, IEmployeePublicService
    {
        private readonly IRepository<HRDbContext, Employment, Guid> _employeeRepository;
        private readonly IRepository<HRDbContext, EmploymentContact, Guid> _employmentContactRepository;
        private readonly IRepository<HRDbContext, EmploymentLocation, Guid> _employeeLocationsRepository;
        private readonly ISpecificationRepository<Employment, Guid> _employeeSpecRepository;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;

        public EmployeeService(IRepository<HRDbContext, Employment, Guid> employeeRepository, IRepository<HRDbContext, EmploymentContact, Guid> employmentContactRepository, ILogger<EmployeeService> logger,
            ISpecificationRepository<Employment, Guid> employeeSpecRepository, IRepository<HRDbContext, EmploymentLocation, Guid> employeeLocationsRepository,
            IUnitOfWork<HRDbContext> uow)
        {
            _employeeRepository = employeeRepository;
            _employmentContactRepository = employmentContactRepository;
            _employeeLocationsRepository = employeeLocationsRepository;
            _employeeSpecRepository = employeeSpecRepository;
            _logger = logger;
            _uow = uow;
        }

        public async Task<Guid> CreateEmployeeAsync(
             string _EmployeeCode,
         Guid _PersonId,
        Guid? _EmploymentTypeId,
        Guid? _EmploymentStatusId,
        DateOnly? _StartDate = null,
        DateOnly? _EndDate = null,

        PhoneNumber? _orgPhone = null,
        Email? _orgEmail = null,
        PhoneNumber? _orgMobile = null
            )
        {
            Employment emp = new Employment(_EmployeeCode, _PersonId, _EmploymentTypeId, _EmploymentStatusId, _StartDate, _EndDate);
            await _employeeRepository.AddAsync(emp);

            await CreateEmploymentContact(HrContactType.OrgMobile, _orgMobile?.Value, emp.Id);
            await CreateEmploymentContact(HrContactType.OfficePhone, _orgPhone?.Value, emp.Id);
            await CreateEmploymentContact(HrContactType.OrgEmail, _orgEmail?.Value, emp.Id);
            return emp.Id;
        }
        private async Task CreateEmploymentContact(HrContactType type, string? value, Guid employmentId)
        {
            if (value != null)
            {
                EmploymentContact contact = new EmploymentContact(type, value, employmentId);
                await _employmentContactRepository.AddAsync(contact);
            }
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
