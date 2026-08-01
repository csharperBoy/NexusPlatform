using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.People;
using Core.Domain.Common.EntityProperties;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Repositories;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Wordprocessing;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Enums;
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

        private readonly IPersonPublicService _personService;
        private readonly IRepository<HRDbContext, Employment, Guid> _employeeRepository;
        private readonly IRepository<HRDbContext, EmployementInfoView, Guid> _employmentInfoRepository;
        private readonly IRepository<HRDbContext, EmploymentContact, Guid> _employmentContactRepository;
        private readonly IRepository<HRDbContext, EmploymentLocation, Guid> _employeeLocationsRepository;
        private readonly ISpecificationRepository<Employment, Guid> _employeeSpecRepository;
        private readonly ISpecificationRepository<EmploymentLocation, Guid> _employmentLocationSpecRepository;
        private readonly ISpecificationRepository<EmploymentContact, Guid> _employmentContactSpecRepository;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;

        public EmployeeService(IPersonPublicService personService,IRepository<HRDbContext, Employment, Guid> employeeRepository, IRepository<HRDbContext, EmploymentContact, Guid> employmentContactRepository, ILogger<EmployeeService> logger,
            ISpecificationRepository<Employment, Guid> employeeSpecRepository, IRepository<HRDbContext, EmploymentLocation, Guid> employeeLocationsRepository,
            IUnitOfWork<HRDbContext> uow, ISpecificationRepository<EmploymentContact, Guid> employmentContactSpecRepository, ISpecificationRepository<EmploymentLocation, Guid> employmentLocationSpecRepository, IRepository<HRDbContext, EmployementInfoView, Guid> employmentInfoRepository)
        {
             _employmentInfoRepository= employmentInfoRepository;
            _personService = personService;
            _employeeRepository = employeeRepository;
            _employmentContactRepository = employmentContactRepository;
            _employeeLocationsRepository = employeeLocationsRepository;
            _employeeSpecRepository = employeeSpecRepository;
            _logger = logger;
            _uow = uow;
            _employmentContactSpecRepository = employmentContactSpecRepository;
            _employmentLocationSpecRepository = employmentLocationSpecRepository;
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
        //private async Task CreateEmploymentContact(HrContactType type, string? value, Guid employmentId)
        //{
        //    if (value != null)
        //    {
        //        EmploymentContact contact = new EmploymentContact(type, value, employmentId);
        //        await _employmentContactRepository.AddAsync(contact);
        //    }
        //}
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
            // ۱. دریافت مکان‌های فعال فعلی کارمند (فرض بر این است که اسپک فقط Activeها را برمی‌گرداند)
            var spec = new GetEmploymentLocationsSpec(employeeId);
            var existingActive = await _employmentLocationSpecRepository.ListBySpecAsync(spec);

            // ۲. مجموعه‌های شناسه‌ها برای مقایسه (حذف تکراری‌های ورودی)
            var existingIds = existingActive.Select(e => e.FkLocationId).ToHashSet();
            var newIds = locationsId.Distinct().ToHashSet();

            // ۳. مکان‌هایی که باید منقضی شوند (موجود اما در لیست جدید نیستند)
            var toExpire = existingActive.Where(e => !newIds.Contains(e.FkLocationId)).ToList();
            foreach (var item in toExpire)
            {
                item.DoExpire();
            }

            // ۴. مکان‌هایی که باید اضافه شوند (در لیست جدید هستند اما قبلاً وجود نداشتند)
            var toAdd = newIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => new EmploymentLocation(id, employeeId))
                .ToList();

            if (toAdd.Any())
            {
                await _employeeLocationsRepository.AddRangeAsync(toAdd);
            }

        }



        public async Task<Guid> UpdateEmploymentAsync(Guid id, string? phone, string? address, string? email, string? mobile, string firstlName, string lastName, DateTime? birthDate, string? birthPlace, string? fatherName, string employeeCode, Guid? employmentTypeId, Guid? employmentStatusId, DateOnly? startDate, DateOnly? endDate, List<Guid> locationsId, string? officePhone, string? orgEmail, string? orgMobile)
        {
            Employment? emp = await _employeeRepository.GetByIdAsync(id);
            if (emp == null)
                throw new Exception("can not found employment!!!");

            bool hasChange = emp.ApplyChange(   employeeCode,  employmentTypeId,  employmentStatusId,  startDate,  endDate);
            if (hasChange)
            {
                await _employeeRepository.UpdateAsync(emp);
            }

            await _personService.UpdatePersonAsync(emp.FkNaturalPersonId, phone, address, email, mobile, firstlName, lastName, birthDate, birthPlace, fatherName);
            if (officePhone != null)
            {
                await CreateEmploymentContact(HrContactType.OfficePhone, officePhone, emp.Id);
            }
            if (orgEmail != null)
            {
                await CreateEmploymentContact(HrContactType.OrgEmail, orgEmail, emp.Id);
            }
            if (orgMobile != null)
            {
                await CreateEmploymentContact(HrContactType.OrgMobile, orgMobile, emp.Id);
            }
            return emp.Id;
        }
        private async Task CreateEmploymentContact(HrContactType type, string? value, Guid employmentId)
        {
            if (value != null)
            {
                GetEmploymentContactSpec spec = new GetEmploymentContactSpec(type, employmentId, value);
                EmploymentContact? existContact = await _employmentContactSpecRepository.GetBySpecAsync(spec);
                if (existContact?.Value.Trim() != value.Trim())
                {
                    if (existContact != null)
                    {
                        existContact.DoExpire();
                        await _employmentContactRepository.UpdateAsync(existContact);

                    }
                    EmploymentContact contact = new EmploymentContact(type, value, employmentId, DateTime.UtcNow);
                    await _employmentContactRepository.AddAsync(contact);
                }

            }
        }

        public async Task<IReadOnlyList<EmployementInfoView>> GetEmploymentListAsync()
        {
            var list = await _employmentInfoRepository.GetAllAsync();
            return list.ToList();
        }
    }
}
