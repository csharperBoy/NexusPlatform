using Contact.Application.DTOs;
using Contact.Application.Interfaces;
using Contact.Application.Mapping;
using Contact.Domain.Entities;
using Contact.Domain.Specifications;
using Contact.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Contact;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.People;
using Core.Shared.DTOs.Contact;
using Core.Shared.DTOs.HR;
using Core.Shared.Enums.Contact;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contact.Infrastructure.Services
{
    public class PhoneBookService : IPhoneBookInternalService, IPhoneBookPublicService
    {
        private readonly ISpecificationRepository<PhoneBookInfoView, Guid> _PhoneBookSpecRepository;
        private readonly ISpecificationRepository<ContactProfileAssignment, Guid> _contactProfileAssignmentSpecRepository;
        private readonly IRepository<ContactDbContext, ContactProfileAssignment, Guid> _contactProfileAssignmentRepository;
        private readonly IEmploymentPublicService _employmentservice;
        private readonly IPostPublicService _postservice;
        private readonly ILocationPublicService _locationservice;
        private readonly IPersonPublicService _personservice;
        private readonly ILogger<PhoneBookService> _logger;

        private readonly IContactInternalService _contactService;
        public PhoneBookService(ILogger<PhoneBookService> logger,
            ISpecificationRepository<PhoneBookInfoView, Guid> PhoneBookSpecRepository,
            IRepository<ContactDbContext, ContactProfileAssignment, Guid> contactProfileAssignmentRepository,
            ISpecificationRepository<ContactProfileAssignment, Guid> contactProfileAssignmentSpecRepository,
            IEmploymentPublicService employmentservice,
            IPostPublicService postservice,
            ILocationPublicService locationservice,
        IPersonPublicService personservice, IContactInternalService contactService

            )
        {
            _PhoneBookSpecRepository = PhoneBookSpecRepository;
            _contactProfileAssignmentSpecRepository = contactProfileAssignmentSpecRepository;
            _contactProfileAssignmentRepository = contactProfileAssignmentRepository;
            _logger = logger;
            _employmentservice = employmentservice;
            _postservice = postservice;
            _locationservice = locationservice;
            _personservice = personservice;
            _contactService = contactService;
        }

        public async Task<IReadOnlyList<PhoneBookEmploymentDto>> GetPhoneBookListAsync(Guid? organUnitId)
        {
            IEnumerable<EmploymentFullDto> empList = await _employmentservice.GetFullInfoAsync();
         
            var existingProfileIds = empList
                .Where(e => e.ProfileId.HasValue)
                .Select(e => e.ProfileId.Value)
                .Concat(empList.Where(e => e.PartyProfileId.HasValue).Select(e => e.PartyProfileId.Value))
                .Concat(empList.SelectMany(e => e.posts.Select(p => p.ProfileId))) 
                .Concat(empList.SelectMany(e => e.empLocations.Select(l => l.ProfileId)))
                .Concat(empList.SelectMany(e => e.postLocations.Select(l => l.ProfileId)))
                .Distinct()
                .ToList();

            List<ContactItemDto> contactList = await _contactService.GetContactsByProfilesIdsAsync(existingProfileIds);
            var employmentDtos = empList.ToPhoneBookDtos(contactList).ToList();
            // ۵. حذف کارمندهایی که هیچ کانتکتی ندارند
            employmentDtos = employmentDtos
                .Where(e => e.Contacts.Any()) // ← شرط جدید
                .ToList();

            // ۵. دریافت پست‌های خالی و مکان‌های خالی (با ارسال existingProfileIds برای جلوگیری از تداخل)
            var standaloneDtos = await GetStandaloneLocationsAndPostsAsync(existingProfileIds);
           
            // ۶. ترکیب نهایی
            var result = employmentDtos
                .Concat(standaloneDtos)
                .ToList();

            return result;            
        }
        private async Task<List<PhoneBookEmploymentDto>> GetStandaloneLocationsAndPostsAsync(List<Guid> existingProfileIds)
        {
            // ۱. پیدا کردن ProfileIdهای پست‌هایی که در لیست کارمندان نیستند ولی کانتکت دارند
            var allAssignments = await _contactProfileAssignmentRepository
                .GetAllAsync(queryOptions:q=>q
                .Where(a => a.IsCurrent)
                .Include(a => a.ContactProfile));

            var postProfileIds = allAssignments
                .Where(a => a.ContactProfile.ProfileType == ContactProfileTypeEnum.Post)
                .Select(a => a.ContactProfileId)
                .Except(existingProfileIds)
                .Distinct()
                .ToList();


            var locationProfileIds = allAssignments
                .Where(a => a.ContactProfile.ProfileType == ContactProfileTypeEnum.Location)
                .Select(a => a.ContactProfileId)
                .Except(existingProfileIds)
                .Distinct()
                .ToList();

            if (!locationProfileIds.Any() && !postProfileIds.Any())
                return new List<PhoneBookEmploymentDto>();

            // ۲. واکشی پست‌ها
            var posts = await _postservice.GetByContactProfileIds(postProfileIds);

            // ۳. دریافت کانتکت‌های این پست‌ها
            var postContactList = await _contactService.GetContactsByProfilesIdsAsync(postProfileIds);

            // ۴. تبدیل به DTO
            var stanAlonePost = posts.Select(post =>
            {
                // کانتکت‌های مربوط به این پست
                var postContacts = postContactList
                    .Where(c => c.ProfileId == post.ProfileId)
                    .Select(c => new ContactDetailDto
                    {
                        Title = c.Label ?? c.ContactType.ToString(),
                        Value = c.Value,
                        Type = c.ContactType,
                        Source = ContactProfileTypeEnum.Post
                    })
                    .ToList();

                // مکان پست (در صورت وجود)
                var currentPostLoc = post.locations;
                var locationTitle = string.Join("-", currentPostLoc?.Select(s => s.Title));

                return new PhoneBookEmploymentDto
                {
                    EmploymentCode = null!,
                    FirstName = null!,
                    LastName = null!,
                    OrganizationUnitsName = post.OrganizationUnitsName ?? "پست بدون سازمان",
                    HeadOfOrganizationUnitsName = post.HeadOfOrganizationUnitsName,
                    JobTitleName = post.JobTitleName ?? "بدون عنوان شغلی",
                    JobLevelTitle = post.JobLevelTitle,
                    LocationTitle = locationTitle,
                    Contacts = postContacts
                };
            }).ToList();


            // ۲. واکشی مکان‌ها
            var locations =  await _locationservice.GetByContactProfileIds(postProfileIds);
       
            var locContactList = await _contactService.GetContactsByProfilesIdsAsync(locationProfileIds);




            // ۴. تبدیل به DTO
            var stanAloneLocation = locations.Select(location =>
            {
                // کانتکت‌های مربوط به این مکان
                var locContacts = locContactList
                    .Where(c => c.ProfileId == location.ProfileId)
                    .Select(c => new ContactDetailDto
                    {
                        Title = c.Label ?? c.ContactType.ToString(),
                        Value = c.Value,
                        Type = c.ContactType,
                        Source = ContactProfileTypeEnum.Location
                    })
                    .ToList();

                return new PhoneBookEmploymentDto
                {
                    EmploymentCode = null!,
                    FirstName = null!,
                    LastName = null!,
                    OrganizationUnitsName = "محل استقرار",
                    HeadOfOrganizationUnitsName = "محل استقرار",
                    JobTitleName = "محل استقرار",
                    JobLevelTitle = null,
                    LocationTitle = location.Title,
                    Contacts = locContacts
                };
            }).ToList();
            var result = stanAlonePost.Concat(stanAloneLocation);
            return result.ToList();
        }
        
        /*public async Task<IReadOnlyList<PhoneBookEmploymentDto>> GetPhoneBookListAsync(Guid? organUnitId)
        {
            // ۱. دریافت اطلاعات کامل کارمندان
            var empList = await _employmentservice.GetFullInfoAsync();

            // ۲. جمع‌آوری تمام ProfileIdهای مرتبط (با SelectMany برای صاف کردن)
            var allProfileIds = empList
                .Select(e => e.ProfileId)
                .Concat(empList.Select(e => e.PartyProfileId))
                .Concat(empList.SelectMany(e => e.posts.Select(p => (Guid?)p.ProfileId)))
                .Concat(empList.SelectMany(e => e.empLocations.Select(l => l.ProfileId)))
                .Concat(empList.SelectMany(e => e.postLocations.Select(l => l.ProfileId)))
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .Distinct()
                .ToList();

            // ۳. دریافت تمام کانتکت‌های مربوط به این ProfileIdها
            var contactList = await _contactService.GetContactsByProfilesIdsAsync(allProfileIds);
            // فرض: contactList از نوع List<ContactDetailDto> است که دارای ProfileId (از نوع Guid) است

            // ۴. گروه‌بندی کانتکت‌ها بر اساس ProfileId
            var contactsLookup = contactList
                .GroupBy(c => c.ProfileId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // ۵. ساخت DTO نهایی
            var result = empList.Select(s =>
            {
                // جمع‌آوری تمام کانتکت‌های مربوط به این کارمند از تمام بخش‌ها
                var allContacts = new List<ContactDetailDto>();

                // کانتکت‌های خود کارمند (ProfileId)
                if (s.ProfileId.HasValue && contactsLookup.TryGetValue(s.ProfileId.Value, out var empContacts))
                    allContacts.AddRange(empContacts);

                // کانتکت‌های شخص (PartyProfileId)
                if (s.PartyProfileId.HasValue && contactsLookup.TryGetValue(s.PartyProfileId.Value, out var partyContacts))
                    allContacts.AddRange(partyContacts);

                // کانتکت‌های پست‌ها
                foreach (var post in s.posts)
                {
                    if (contactsLookup.TryGetValue(post.ProfileId, out var postContacts))
                        allContacts.AddRange(postContacts);
                }

                // کانتکت‌های مکان‌های کاری (empLocations)
                foreach (var loc in s.empLocations)
                {
                    if (loc.ProfileId.HasValue && contactsLookup.TryGetValue(loc.ProfileId.Value, out var locContacts))
                        allContacts.AddRange(locContacts);
                }

                // کانتکت‌های مکان‌های پست (postLocations)
                foreach (var loc in s.postLocations)
                {
                    if (loc.ProfileId.HasValue && contactsLookup.TryGetValue(loc.ProfileId.Value, out var locContacts))
                        allContacts.AddRange(locContacts);
                }

                // حذف کانتکت‌های تکراری بر اساس ترکیب Type, Value, Source
                var distinctContacts = allContacts
                    .GroupBy(c => new { c.Type, c.Value, c.Source })
                    .Select(g => g.First())
                    .ToList();

                return new PhoneBookEmploymentDto
                {
                    EmploymentCode = s.EmploymentCode,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    OrganizationUnitsName = string.Join(" - ", s.posts.Select(p => p.OrganizationUnitsName).Distinct()),
                    HeadOfOrganizationUnitsName = string.Join(" - ", s.posts.Select(p => p.HeadOfOrganizationUnitsName).Distinct()),
                    JobTitleName = string.Join(" - ", s.posts.Select(p => p.JobTitleName).Distinct()),
                    JobLevelTitle = string.Join(" - ", s.posts.Select(p => p.JobLevelTitle).Distinct()),
                    LocationTitle = string.Join(" - ",
                        s.empLocations.Select(l => l.Title)
                        .Concat(s.postLocations.Select(l => l.Title))
                        .Distinct()),
                    Contacts = distinctContacts
                };
            }).ToList();

            return result;
        }
        */
        //public async Task<IReadOnlyList<PhoneBookEmploymentDto>> GetPhoneBookListAsync(Guid? organUnitId)
        //{
        //    GetPhoneBookSpec spec = new GetPhoneBookSpec();
        //    var list = await _PhoneBookSpecRepository.ListBySpecAsync(spec);
        //    IReadOnlyList<PhoneBookEmploymentDto> result = list.ToPhoneBookDtos();
        //    return result;
        //}
    }
}
