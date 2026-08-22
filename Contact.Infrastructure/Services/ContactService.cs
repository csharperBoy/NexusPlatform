using Contact.Application.Interfaces;
using Contact.Domain.Entities;
using Contact.Domain.Specifications;
using Contact.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Domain.Common.EntityProperties;
using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;

using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using HR.Domain.Entities;
using HR.Domain.Specifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Infrastructure.Services
{
    public class ContactService : IContactInternalService
    {
        private readonly IRepository<ContactDbContext, ContactResource, Guid> _contactResourceRepository;
        private readonly ISpecificationRepository<ContactResource, Guid> _contactResourceSpecRepository;

        private readonly IRepository<ContactDbContext, ContactProfile, Guid> _contactProfileRepository;
        private readonly ISpecificationRepository<ContactProfile, Guid> _contactProfileSpecRepository;

        private readonly IRepository<ContactDbContext, ContactProfileAssignment, Guid> _assignmentRepository;
        private readonly ISpecificationRepository<ContactProfileAssignment, Guid> _assignmentSpecRepository;

        private readonly IUnitOfWork<ContactDbContext> _uow;
        private readonly ILogger<ContactService> _logger;



        public ContactService(ILogger<ContactService> logger,
             IRepository<ContactDbContext, ContactResource, Guid> contactResourceRepository,
         ISpecificationRepository<ContactResource, Guid> contactResourceSpecRepository,
 IRepository<ContactDbContext, ContactProfile, Guid> contactProfileRepository,
         ISpecificationRepository<ContactProfile, Guid> contactProfileSpecRepository,


        IRepository<ContactDbContext, ContactProfileAssignment, Guid> assignmentRepository,
        ISpecificationRepository<ContactProfileAssignment, Guid> assignmentSpecRepository,

        IUnitOfWork<ContactDbContext> uow)
        {
            _contactResourceRepository = contactResourceRepository;
            _contactResourceSpecRepository = contactResourceSpecRepository;
            _assignmentRepository = assignmentRepository;
            _assignmentSpecRepository = assignmentSpecRepository;
            _contactProfileRepository = contactProfileRepository;
            _contactProfileSpecRepository = contactProfileSpecRepository;

            _logger = logger;
            _uow = uow;
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }



        //public async Task CreateContact(ContactTypeEnum type, List<string>? values, Guid profileId)
        //{
        //    if (values != null)
        //    {
        //        // ۱. دریافت مکان‌های فعال فعلی کارمند (فرض بر این است که اسپک فقط Activeها را برمی‌گرداند)               
        //        GetContactResourceSpec spec = new GetContactResourceSpec(type, profileId);
        //        IEnumerable<ContactResource>? existContact = await _contactItemSpecRepository.ListBySpecAsync(spec);

        //        // ۲. مجموعه‌های شناسه‌ها برای مقایسه (حذف تکراری‌های ورودی)
        //        var existingValues = existContact.Select(e => e.Value).ToHashSet();
        //        var newValues = values.Distinct().ToHashSet();

        //        // ۳. مکان‌هایی که باید منقضی شوند (موجود اما در لیست جدید نیستند)
        //        var toExpire = existContact.Where(e => !newValues.Contains(e.Value)).ToList();
        //        foreach (var item in toExpire)
        //        {
        //            await item.DoExpire();
        //        }

        //        // ۴. مکان‌هایی که باید اضافه شوند (در لیست جدید هستند اما قبلاً وجود نداشتند)
        //        var toAdd = newValues
        //            .Where(val => !existingValues.Contains(val))
        //            .Select(val => new ContactResource(type, val, profileId, DateTime.UtcNow))
        //            .ToList();

        //        if (toAdd.Any())
        //        {
        //            await _contactItemRepository.AddRangeAsync(toAdd);
        //        }

        //    }
        //}
        public async Task SyncProfileContacts(ContactTypeEnum type, List<string>? values, Guid profileId)
        {
            var newValues = values?.Distinct().ToHashSet() ?? new HashSet<string>();

            // ۱. دریافت انتساب‌های فعال فعلی این پروفایل
            var assignmentSpec = new GetProfileAssignmentsSpec(type, profileId);
            var activeAssignments = await _assignmentSpecRepository.ListBySpecAsync(assignmentSpec);

            var existingValues = activeAssignments.Select(a => a.ContactResource.Value).ToHashSet();

            // ۲. منقضی کردن انتساب‌هایی که در لیست جدید نیستند
            var assignmentsToExpire = activeAssignments
                .Where(a => !newValues.Contains(a.ContactResource.Value))
                .ToList();

            foreach (var assignment in assignmentsToExpire)
            {
                assignment.DoExpire(); // غیرفعال کردن انتساب (IsCurrent = false, EffectiveTo = UtcNow)
            }

            // ۳. پردازش مقادیر جدید که باید منتسب شوند
            var valuesToAdd = newValues.Where(val => !existingValues.Contains(val)).ToList();

            foreach (var val in valuesToAdd)
            {
                // الف) بررسی وجود شماره در کاتالوگ کلی سیستم
                var resourceSpec = new GetContactResourceSpec(type, val);
                var resource = await _contactResourceSpecRepository.GetBySpecAsync(resourceSpec);

                // ب) اگر در کاتالوگ وجود نداشت، ثبت در کاتالوگ
                if (resource == null)
                {
                    resource = new ContactResource(type, val);
                    await _contactResourceRepository.AddAsync(resource);
                }

                // ج) ایجاد انتساب جدید بین پروفایل و شماره کاتالوگ
                var newAssignment = new ContactProfileAssignment(profileId, resource.Id, DateTime.UtcNow);
                await _assignmentRepository.AddAsync(newAssignment);
            }
        }

        public async Task<Guid> CreateContactProfileAsync(string Title, ContactProfileTypeEnum Type, CancellationToken cancellationToken = default)
        {
            var newModel = new ContactProfile(Title, Type);
            await _contactProfileRepository.AddAsync(newModel);
            return newModel.Id;
        }

        public async Task<List<ContactItemDto>> GetContactsByProfilesIdsAsync(List<Guid> profilesId)
        {
            var spec = new GetContactsByProfileIdsSpec(profilesId);

            // کوئری روی مخزن انتساب‌ها (Assignment Repository)
            var assignments = await _assignmentSpecRepository.ListBySpecAsync(spec);

            return assignments.Select(a => new ContactItemDto
            {
                ProfileId = a.ContactProfileId,
                ContactType = a.ContactResource.ContactType,
                Value = a.ContactResource.Value,
                Label = a.ContactResource.Label,

                // اطلاعات مربوط به زمان و وضعیت انتساب
                EffectiveFrom = a.EffectiveFrom,
                EffectiveTo = a.EffectiveTo,
                IsCurrent = a.IsCurrent,

                // مپ کردن موارد وابسته/فرزند از کاتالوگ منبع
                ChildContactItems = a.ContactResource.ChildContactResources != null && a.ContactResource.ChildContactResources.Any()
                    ? a.ContactResource.ChildContactResources.Select(c => new ContactItemDto
                    {
                        Value = c.Value,
                        Label = c.Label,
                        ContactType = c.ContactType
                    }).ToList()
                    : null
            }).ToList();
        }
        public async Task DeActiveContactProfileAsync(Guid ProfileId)
        {
            ContactProfile? profile = await _contactProfileRepository.GetByIdAsync(ProfileId);
            if (profile == null)
                throw new Exception("Can Not Found Profile!!!");
            profile.DeActive();
        }
        public async Task ExpireAllContactAsync(Guid profileId)
        {
            // ۱. دریافت تمام انتساب‌های فعالِ این پروفایل مشخص
            IEnumerable<ContactProfileAssignment> activeAssignments = await _assignmentRepository.GetAllAsync(
                queryOptions: queryOptions => queryOptions.Where(a => a.ContactProfileId == profileId && a.IsCurrent)
            );

            // ۲. منقضی کردن انتساب‌ها (غیرفعال کردن رابطه)
            foreach (var assignment in activeAssignments)
            {
                assignment.DoExpire(); // تنظیم IsCurrent = false و EffectiveTo = UtcNow
            }
        }
    }
}
