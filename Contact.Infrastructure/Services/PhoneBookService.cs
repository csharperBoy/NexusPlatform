using Contact.Application.DTOs;
using Contact.Application.Interfaces;
using Contact.Application.Mapping;
using Contact.Domain.Entities;
using Contact.Domain.Helper;
using Contact.Domain.Specifications;
using Contact.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Application.Abstractions.Caching.PublicService;
using Core.Application.Abstractions.Contact;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.People;
using Core.Shared.DTOs.Contact;
using Core.Shared.DTOs.HR;
using Core.Shared.Enums;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using HR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
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
        private readonly ICachePublicService _cacheService;

        private readonly IContactInternalService _contactService;
        public PhoneBookService(ILogger<PhoneBookService> logger,
            ISpecificationRepository<PhoneBookInfoView, Guid> PhoneBookSpecRepository,
            IRepository<ContactDbContext, ContactProfileAssignment, Guid> contactProfileAssignmentRepository,
            ISpecificationRepository<ContactProfileAssignment, Guid> contactProfileAssignmentSpecRepository,
            IEmploymentPublicService employmentservice,
            IPostPublicService postservice,
            ILocationPublicService locationservice,
             ICachePublicService cacheService,
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
            _cacheService = cacheService;
        }

        public async Task<IReadOnlyList<PhoneBookEmploymentDto>> GetPhoneBookListAsync(Guid? organUnitId)
        {
            var cacheKey = CacheKeyHelper.PhoneBook_GetPhoneBookList;

            //var cached = await _cacheService.GetAsync<IReadOnlyList<PhoneBookEmploymentDto>>(cacheKey);
            //if (cached != null)
            //{
            //    _logger.LogDebug("Cache hit for full Get PhoneBook List");
            //    return cached;
            //}

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

            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromDays(30));

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
                        //Title = c.Label ?? c.ContactType.ToString(),
                        Title = string.IsNullOrWhiteSpace(c.Label) ? c.ContactType.GetPersianDescription() : c.Label,

                        Value = c.Value,
                        Type = c.ContactType,
                        Source = ContactProfileTypeEnum.Post
                    })
                    .ToList();

                // مکان پست (در صورت وجود)
                var currentPostLoc = post.locations;
                var locationTitles = currentPostLoc != null ?  currentPostLoc.Select(s=>s.Title).ToList(): new List<string>();

                return new PhoneBookEmploymentDto
                {
                    uniqueKey = $"post-{post.Id}",
                    EmploymentCode = null!,
                    FirstName = null!,
                    LastName = null!,
                    OrganizationUnitsName = string.IsNullOrEmpty(post.OrganizationUnitsName) ? new List<string>() : new List<string> { post.OrganizationUnitsName },
                    HeadOfOrganizationUnitsName = string.IsNullOrEmpty(post.HeadOfOrganizationUnitsName) ? new List<string>() : new List<string> { post.HeadOfOrganizationUnitsName },
                    JobTitleName = string.IsNullOrEmpty(post.JobTitleName) ? new List<string>() : new List<string> { post.JobTitleName },
                    JobLevelTitle = string.IsNullOrEmpty(post.JobLevelTitle) ? new List<string>() : new List<string> { post.JobLevelTitle },
                    LocationTitle = locationTitles,
                    Gender = Gender.Other,
                    Contacts = postContacts
                };
            }).ToList();


            // ۲. واکشی مکان‌ها
            var locations =  await _locationservice.GetByContactProfileIds(locationProfileIds);
       
            var locContactList = await _contactService.GetContactsByProfilesIdsAsync(locationProfileIds);




            // ۴. تبدیل به DTO
            var stanAloneLocation = locations.Select(location =>
            {
                // کانتکت‌های مربوط به این مکان
                var locContacts = locContactList
                    .Where(c => c.ProfileId == location.ProfileId)
                    .Select(c => new ContactDetailDto
                    {
                        //Title = c.Label ?? c.ContactType.ToString(),
                        Title = string.IsNullOrWhiteSpace(c.Label) ? c.ContactType.GetPersianDescription() : c.Label,

                        Value = c.Value,
                        Type = c.ContactType,
                        Source = ContactProfileTypeEnum.Location
                    })
                    .ToList();

                return new PhoneBookEmploymentDto
                {

                    uniqueKey = $"loc-{location.Id}",
                    EmploymentCode = null!,
                    FirstName = null!,
                    LastName = null!,
                    OrganizationUnitsName = new List<string> { "محل استقرار" },
                    HeadOfOrganizationUnitsName = new List<string> { "محل استقرار" },
                    JobTitleName = new List<string> { "محل استقرار" },
                    JobLevelTitle = new List<string>(),
                    LocationTitle = new List<string> { location.Title },
                    Gender = Gender.Other,
                    Contacts = locContacts
                };
            }).ToList();
            var result = stanAlonePost.Concat(stanAloneLocation);
            return result.ToList();
        }
        
        
    }
}
