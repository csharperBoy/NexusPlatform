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
    public class EmploymentService : IEmploymentInternalService, IEmploymentPublicService
    {

        private readonly IPersonPublicService _personService;
        private readonly IRepository<HRDbContext, Employment, Guid> _employmentRepository;
        private readonly IRepository<HRDbContext, EmployementInfoView, Guid> _employmentInfoRepository;
        private readonly IRepository<HRDbContext, EmploymentContact, Guid> _employmentContactRepository;
        private readonly IRepository<HRDbContext, EmploymentLocation, Guid> _employmentLocationsRepository;
        private readonly ISpecificationRepository<Employment, Guid> _employmentSpecRepository;
        private readonly ISpecificationRepository<EmploymentLocation, Guid> _employmentLocationSpecRepository;
        private readonly ISpecificationRepository<EmploymentContact, Guid> _employmentContactSpecRepository;
        private readonly ILogger<EmploymentService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;

        public EmploymentService(IPersonPublicService personService, IRepository<HRDbContext, Employment, Guid> employmentRepository, IRepository<HRDbContext, EmploymentContact, Guid> employmentContactRepository, ILogger<EmploymentService> logger,
            ISpecificationRepository<Employment, Guid> employmentSpecRepository, IRepository<HRDbContext, EmploymentLocation, Guid> employmentLocationsRepository,
            IUnitOfWork<HRDbContext> uow, ISpecificationRepository<EmploymentContact, Guid> employmentContactSpecRepository, ISpecificationRepository<EmploymentLocation, Guid> employmentLocationSpecRepository, IRepository<HRDbContext, EmployementInfoView, Guid> employmentInfoRepository)
        {
            _employmentInfoRepository = employmentInfoRepository;
            _personService = personService;
            _employmentRepository = employmentRepository;
            _employmentContactRepository = employmentContactRepository;
            _employmentLocationsRepository = employmentLocationsRepository;
            _employmentSpecRepository = employmentSpecRepository;
            _logger = logger;
            _uow = uow;
            _employmentContactSpecRepository = employmentContactSpecRepository;
            _employmentLocationSpecRepository = employmentLocationSpecRepository;
        }

        public async Task<Guid> CreateEmploymentAsync(
             string _EmploymentCode,
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
            Employment emp = new Employment(_EmploymentCode, _PersonId, _EmploymentTypeId, _EmploymentStatusId, _StartDate, _EndDate);
            await _employmentRepository.AddAsync(emp);

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
        public async Task<Guid?> GetEmploymentId(Guid? personId)
        {
            GetEmploymentByPersonIdSpec spec = new GetEmploymentByPersonIdSpec(personId);
            Employment? employment = await _employmentSpecRepository.GetBySpecAsync(spec);
            if (employment == null)
                throw new InvalidOperationException("employment not found!!!");

            return employment.Id;

        }

        public async Task AssignLocationsToEmployment(Guid employmentId, List<Guid> locationsId)
        {
            // ۱. دریافت مکان‌های فعال فعلی کارمند (فرض بر این است که اسپک فقط Activeها را برمی‌گرداند)
            var spec = new GetEmploymentLocationsSpec(employmentId);
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
                .Select(id => new EmploymentLocation(id, employmentId))
                .ToList();

            if (toAdd.Any())
            {
                await _employmentLocationsRepository.AddRangeAsync(toAdd);
            }

        }



        public async Task<Guid> UpdateEmploymentAsync(Guid id, string? phone, string? address, string? email, string? mobile, string? firstlName, string? lastName, DateTime? birthDate, string? birthPlace, string? fatherName, string? employmentCode, Guid? employmentTypeId, Guid? employmentStatusId, DateOnly? startDate, DateOnly? endDate, List<Guid>? locationsId, string? officePhone, string? orgEmail, string? orgMobile)
        {
            Employment? emp = await _employmentRepository.GetByIdAsync(id);
            if (emp == null)
                throw new Exception("can not found employment!!!");

            bool hasChange = emp.ApplyChange(employmentCode, employmentTypeId, employmentStatusId, startDate, endDate);
            if (hasChange)
            {
                await _employmentRepository.UpdateAsync(emp);
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
                        await existContact.DoExpire();
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
