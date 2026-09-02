using Core.Application.Abstractions;
using Core.Application.Abstractions.Contact;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.People;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.ValueObjects;
using Core.Shared.DTOs.HR;
using Core.Shared.Enums;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Events.Employment;
using HR.Domain.Specifications;
using HR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace HR.Infrastructure.Services
{
    public class EmploymentService : IEmploymentInternalService, IEmploymentPublicService
    {
        private readonly IPersonPublicService _personService;
        private readonly IRepository<HRDbContext, Employment, Guid> _employmentRepository;
        private readonly IRepository<HRDbContext, Assignment, Guid> _assignmentRepository;
        private readonly IRepository<HRDbContext, EmploymentInfoView, Guid> _employmentInfoRepository;
        private readonly IRepository<HRDbContext, PostLocation, Guid> _postLocationsRepository;
        private readonly IRepository<HRDbContext, EmploymentLocation, Guid> _employmentLocationsRepository;
        private readonly ISpecificationRepository<EmploymentLocation, Guid> _employmentLocationSpecRepository;
        private readonly ISpecificationRepository<Employment, Guid> _employmentSpecRepository;
        private readonly IContactPublicService _contactService;
        private readonly ILogger<EmploymentService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;

        public EmploymentService(IPersonPublicService personService,
            IRepository<HRDbContext, Assignment, Guid> assignmentRepository,
            IRepository<HRDbContext, PostLocation, Guid> postLocationsRepository,
             IRepository<HRDbContext, EmploymentLocation, Guid> employmentLocationsRepository,
             ISpecificationRepository<EmploymentLocation, Guid> employmentLocationSpecRepository,
            IRepository<HRDbContext, Employment, Guid> employmentRepository,
            ILogger<EmploymentService> logger,
            ISpecificationRepository<Employment, Guid> employmentSpecRepository,
           IContactPublicService contactService,
            IUnitOfWork<HRDbContext> uow,
            IRepository<HRDbContext, EmploymentInfoView, Guid> employmentInfoRepository
            )
        {
            _postLocationsRepository = postLocationsRepository;
            _employmentInfoRepository = employmentInfoRepository;
            _assignmentRepository = assignmentRepository;
            _contactService = contactService;
            _personService = personService;
            _employmentRepository = employmentRepository;
            _employmentLocationsRepository = employmentLocationsRepository;
            _employmentSpecRepository = employmentSpecRepository;
            _logger = logger;
            _uow = uow;
            _employmentLocationSpecRepository = employmentLocationSpecRepository;
        }

        public async Task<Guid> CreateEmploymentAsync(
             string _EmploymentCode,
             Guid _PersonId,
             Guid? _EmploymentTypeId,
             Guid? _EmploymentStatusId,
             DateOnly? _StartDate = null,
             DateOnly? _EndDate = null,
             List<PhoneNumber>? _orgPhone = null,
             List<Email>? _orgEmail = null,
             List<PhoneNumber>? _orgMobile = null
            )
        {
            Employment? existEmp = (await _employmentRepository.GetAllAsync(queryOptions: q => q.Where(a => a.EmploymentCode.Trim() == _EmploymentCode.Trim()))).FirstOrDefault();
            Employment emp;
            if (existEmp == null)
            {
                Guid contactProfileId = await _contactService.CreateContactProfileAsync($"Employment - {_EmploymentCode}", ContactProfileTypeEnum.Employment);
                emp = new Employment(_EmploymentCode, _PersonId, contactProfileId, _EmploymentTypeId, _EmploymentStatusId, _StartDate, _EndDate);

                await _employmentRepository.AddAsync(emp);

                await _contactService.SyncProfileContacts(ContactTypeEnum.OrganizationMobile, _orgMobile?.Select(t => t.Value).ToList(), emp.FkContactProfileId);
                await _contactService.SyncProfileContacts(ContactTypeEnum.OfficePhone, _orgPhone?.Select(t => t.Value).ToList(), emp.FkContactProfileId);
                await _contactService.SyncProfileContacts(ContactTypeEnum.Email, _orgEmail?.Select(t => t.Value).ToList(), emp.FkContactProfileId);
                return emp.Id;
            }
            else
            {
                emp = new Employment(_EmploymentCode, _PersonId, existEmp.FkContactProfileId, _EmploymentTypeId, _EmploymentStatusId, _StartDate, _EndDate);

                existEmp.ApplyChange(emp,
                    new List<string> {
                    "Employment.EmploymentCode",
                    "Employment.FkNaturalPersonId",
                    "Employment.FkEmploymentTypeId",
                    "Employment.FkEmploymentStatusId",
                    "Employment.FkContactProfileId",
                    "Employment.EffectiveFrom",
                    "Employment.EffectiveTo"
                });


                await existEmp.SetIsRemove(false);
                await _employmentRepository.UpdateAsync(existEmp);
                return existEmp.Id;
            }
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
            await _contactService.SaveAsync();
            await _personService.SaveAsync();
        }
        public async Task<Guid?> GetEmploymentId(Guid? personId)
        {
            GetEmploymentByPersonIdSpec spec = new GetEmploymentByPersonIdSpec(personId);
            Employment? employment = await _employmentSpecRepository.GetBySpecAsync(spec);
            if (employment == null)
                //throw new InvalidOperationException("employment not found!!!");
                return null;

            return employment.Id;

        }

        public async Task<bool> AssignLocationsToEmployment(Guid employmentId, List<Guid> locationsId)
        {
            bool hasChange = false;
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
                hasChange = true;
            }

            // ۴. مکان‌هایی که باید اضافه شوند (در لیست جدید هستند اما قبلاً وجود نداشتند)
            var toAdd = newIds
                .Where(id => !existingIds.Contains(id))
                .Select(id => new EmploymentLocation(id, employmentId))
                .ToList();

            if (toAdd.Any())
            {
                await _employmentLocationsRepository.AddRangeAsync(toAdd);
                foreach (var item in toAdd)
                {
                    item.AddDomainEvent(new ChangeEmploymentEvent(item.Id));

                }
                hasChange = true;
            }
            return hasChange;
        }



        public async Task<bool> UpdateEmploymentAsync(
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
          Optional<List<string>?> orgMobile
            )
        {
            Employment? emp = await _employmentRepository.GetByIdAsync(id);
            if (emp == null)
                throw new Exception("can not found employment!!!");

            bool hasChange = emp.ApplyChange(employmentCode, employmentTypeId, employmentStatusId, startDate, endDate);
            //bool hasChange = emp.ApplyChange(new Employment(employmentCode, employmentTypeId, employmentStatusId, startDate, endDate), UpdateMask);
            if (hasChange)
            {
                await _employmentRepository.UpdateAsync(emp);
            }

            List<PhoneNumber>? Phones = new List<PhoneNumber>();
            List<Email>? Emails = new List<Email>();
            List<PhoneNumber>? Mobiles = new List<PhoneNumber>();
            try { Phones.AddRange(phone.IsSet ? phone.Value?.Select(a => PhoneNumber.Create(a)).ToList() : null); } catch { }
            try { Emails.AddRange(email.IsSet ? email.Value?.Select(a => Email.Create(a)).ToList() : null); } catch { }
            try { Mobiles.AddRange(mobile.IsSet ? mobile.Value?.Select(a => PhoneNumber.Create(a)).ToList() : null); } catch { }

            bool personHasChange = await _personService.UpdatePersonAsync(emp.FkNaturalPersonId, firstName, lastName, birthDate, birthPlace, fatherName, nationalCode,

                Phones, address, Emails, Mobiles
                );
            if (officePhone.IsSet)
            {
               bool contactChange= await _contactService.SyncProfileContacts(ContactTypeEnum.OfficePhone, officePhone.Value, emp.FkContactProfileId);
                hasChange = (hasChange || contactChange);

            }
            if (orgEmail.IsSet)
            {
                bool contactChange = await _contactService.SyncProfileContacts(ContactTypeEnum.Email, orgEmail.Value, emp.FkContactProfileId);
                hasChange = (hasChange || contactChange);
            }
            if (orgMobile.IsSet)
            {
                bool contactChange = await _contactService.SyncProfileContacts(ContactTypeEnum.OrganizationMobile, orgMobile.Value, emp.FkContactProfileId);
                hasChange = (hasChange || contactChange);
            }
            if (hasChange)
                emp.AddDomainEvent(new ChangeEmploymentEvent(emp.Id));
            return (hasChange || personHasChange);
        }


        public async Task<IReadOnlyList<EmploymentInfoDto>> GetEmploymentListAsync()
        {
            var empList = await _employmentInfoRepository.GetAllAsync();
            var emptIds = empList.Select(p => p.Id).ToList();
            var locList = await _employmentLocationsRepository.GetAllAsync(queryOptions: q =>
                q.Where(a => emptIds.Contains(a.FkEmploymentId) && a.IsCurrent)
                 .Include(a => a.Location)
            );

            var result = empList.Select(s => new EmploymentInfoDto
            {
                EmploymentCode = s.EmploymentCode,
                FirstName = s.FirstName,
                LastName = s.LastName,
                CostCenterName = s.CostCenterName,
                Id = s.Id,
                GradeTitle = s.GradeTitle,
                JobLevelTitle = s.JobLevelTitle,
                JobTitleName = s.JobTitleName,
                OrganizationUnitsName = s.OrganizationUnitsName,
                PostCode = s.PostCode,
                //Contacts = hrContactList.Where(l=>l.IsCurrent && l.EntityId == s.Id ).ToList(),
                locations = locList.Where(l => l.FkEmploymentId == s.Id).Select(s => new LocationInfoDto { Id = s.Location.Id, Title = s.Location.Title }).ToList(),
                AssignmentsAssigneeType = s.AssignmentsAssigneeType,
                Gender = s.Gender.ToString().ToEnumOrDefault<Gender>(Gender.Other),
                AssignmentsEffectiveFrom = s.AssignmentsEffectiveFrom,
                AssignmentsEffectiveTo = s.AssignmentsEffectiveTo,
                EmploymentEffectiveFrom = s.EmploymentEffectiveFrom,
                EmploymentEffectiveTo = s.EmploymentEffectiveTo,
                EmploymentStatusName = s.EmploymentStatusName,
                EmploymentTypeName = s.EmploymentTypeName,
                NationalCode = s.NationalCode,
                ProfileId = s.FkContactProfileId,
                PartyProfileId = s.FkPartyContactProfileId,
                PartyId = s.PartyId,
                //partyContacts = peopleContactList.Where(l => l.IsCurrent && l.EntityId == s.PartyId).ToList(),

            }).ToList();
            return result;
        }

        public async Task DeleteAsync(Guid id)
        {
            Employment? model = await _employmentRepository.GetByIdAsync(id);
            if (model == null)
                throw new Exception("can not found employment!!!");

            await model.SoftRemove();
            model.AddDomainEvent(new RemoveEmploymentEvent(model.Id, model.EmploymentCode, model.FkNaturalPersonId, model.FkEmploymentTypeId, model.FkEmploymentStatusId, model.FkContactProfileId));
            await ExpireEmploymentPostsAsync(id);


            await ExpireEmploymentLocationsAsync(id);

        }

        private async Task ExpireEmploymentLocationsAsync(Guid id)
        {
            var locList = await _employmentLocationsRepository.GetAllAsync(queryOptions: q => q.Where(a => a.FkEmploymentId == id && a.IsCurrent));
            foreach (var item in locList)
            {
                item.DoExpire();
            }
        }

        private async Task ExpireEmploymentPostsAsync(Guid id)
        {
            var postList = await _assignmentRepository.GetAllAsync(queryOptions: q => q.Where(a => a.FkEmploymentId == id && a.IsCurrent));
            foreach (var item in postList)
            {
                item.DoExpire();
            }
        }

        public async Task<IEnumerable<EmploymentFullDto>> GetFullInfoAsync()
        {
            var empList = await _employmentInfoRepository.GetAllAsync();
            var emptIds = empList.Select(p => p.Id).ToList();
            var postsAssign = await _assignmentRepository.GetAllAsync(queryOptions:
                q => q.Where(a => emptIds.Contains(a.FkEmploymentId) && a.IsCurrent)
                .Include(p => p.Post).ThenInclude(p => p.OrganizationUnit).ThenInclude(p => p.Parent)
                .Include(p => p.Post).ThenInclude(p => p.CostCenter)
                .Include(p => p.Post).ThenInclude(p => p.Grade)
                .Include(p => p.Post).ThenInclude(p => p.JobTitle)
                .Include(p => p.Post).ThenInclude(p => p.JobLevel)

                );
            var postIds = postsAssign.Select(p => p.FkPostId).ToList();
            var empLocList = await _employmentLocationsRepository.GetAllAsync(queryOptions: q =>
            q.Where(a => emptIds.Contains(a.FkEmploymentId) && a.IsCurrent)
                 .Include(a => a.Location)
            );
            var postLocList = await _postLocationsRepository.GetAllAsync(queryOptions: q => q.Where(a => postIds.Contains(a.FkPostId) && a.IsCurrent)
                 .Include(a => a.Post).Include(a => a.Location)
            );

            var result = empList.Select(s => new EmploymentFullDto
            {
                EmploymentCode = s.EmploymentCode,
                FirstName = s.FirstName,
                LastName = s.LastName,
                Id = s.Id,

                EmploymentEffectiveFrom = s.EmploymentEffectiveFrom,
                EmploymentEffectiveTo = s.EmploymentEffectiveTo,
                EmploymentStatusName = s.EmploymentStatusName,
                EmploymentTypeName = s.EmploymentTypeName,
                NationalCode = s.NationalCode,
                Gender = s.Gender.ToString().ToEnumOrDefault<Gender>(Gender.Other),
                ProfileId = s.FkContactProfileId,
                PartyProfileId = s.FkPartyContactProfileId,
                PartyId = s.PartyId,
                empLocations = empLocList.Where(l => l.FkEmploymentId == s.Id).Select(s => new LocationInfoDto { Id = s.Location.Id, Title = s.Location.Title, ProfileId = s.Location.FkContactProfileId }).ToList(),
                postLocations = postLocList.Where(l => l.FkPostId == s.Id).Select(s => new LocationInfoDto { Id = s.Location.Id, Title = s.Location.Title, ProfileId = s.Location.FkContactProfileId }).ToList(),
                posts = postsAssign.Where(p => p.FkEmploymentId == s.Id).Select(s => s.Post).ToList().Select(p => new PostInfoDto
                {
                    Id = p.Id,
                    PostCode = p.Code,
                    JobTitleName = p.JobTitle.Name,
                    JobLevelTitle = p.JobLevel?.Title,
                    OrganizationUnitsName = p.OrganizationUnit?.Name,
                    HeadOfOrganizationUnitsName = p.OrganizationUnit?.Parent?.Name ?? p.OrganizationUnit?.Name,
                    GradeTitle = p.Grade?.Title,
                    ProfileId = p.FkContactProfileId,
                    CostCenterName = p.CostCenter?.Name
                }).ToList()
            }).ToList();
            return result;
        }


    }
}
