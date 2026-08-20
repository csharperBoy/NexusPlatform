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
        private readonly IRepository<ContactDbContext, ContactResource, Guid> _contactItemRepository;
        private readonly ISpecificationRepository<ContactResource, Guid> _contactItemSpecRepository;

        private readonly IRepository<ContactDbContext, ContactProfile, Guid> _contactProfileRepository;
        private readonly ISpecificationRepository<ContactProfile, Guid> _contactProfileSpecRepository;

        private readonly IUnitOfWork<ContactDbContext> _uow;
        private readonly ILogger<ContactService> _logger;



        public ContactService(ILogger<ContactService> logger,
             IRepository<ContactDbContext, ContactResource, Guid> contactItemRepository,
         ISpecificationRepository<ContactResource, Guid> contactItemSpecRepository,
 IRepository<ContactDbContext, ContactProfile, Guid> contactProfileRepository,
         ISpecificationRepository<ContactProfile, Guid> contactProfileSpecRepository,


        IUnitOfWork<ContactDbContext> uow)
        {
            _contactItemRepository = contactItemRepository;
            _contactItemSpecRepository = contactItemSpecRepository;

            _contactProfileRepository = contactProfileRepository;
            _contactProfileSpecRepository = contactProfileSpecRepository;

            _logger = logger;
            _uow = uow;
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }



        public async Task CreateContact(ContactTypeEnum type, List<string>? values, Guid profileId)
        {
            if (values != null)
            {
                // ۱. دریافت مکان‌های فعال فعلی کارمند (فرض بر این است که اسپک فقط Activeها را برمی‌گرداند)               
                GetContactSpec spec = new GetContactSpec(type, profileId);
                IEnumerable<ContactResource>? existContact = await _contactItemSpecRepository.ListBySpecAsync(spec);

                // ۲. مجموعه‌های شناسه‌ها برای مقایسه (حذف تکراری‌های ورودی)
                var existingValues = existContact.Select(e => e.Value).ToHashSet();
                var newValues = values.Distinct().ToHashSet();

                // ۳. مکان‌هایی که باید منقضی شوند (موجود اما در لیست جدید نیستند)
                var toExpire = existContact.Where(e => !newValues.Contains(e.Value)).ToList();
                foreach (var item in toExpire)
                {
                    await item.DoExpire();
                }

                // ۴. مکان‌هایی که باید اضافه شوند (در لیست جدید هستند اما قبلاً وجود نداشتند)
                var toAdd = newValues
                    .Where(val => !existingValues.Contains(val))
                    .Select(val => new ContactResource(type, val, profileId, DateTime.UtcNow))
                    .ToList();

                if (toAdd.Any())
                {
                    await _contactItemRepository.AddRangeAsync(toAdd);
                }

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
            GetContactsByProfileIdsSpec spec = new GetContactsByProfileIdsSpec(profilesId);
            var list = await _contactItemSpecRepository.ListBySpecAsync(spec);
            return list.Select(c => new ContactItemDto
            {
                ProfileId = c.ContactProfileId,
                ContactType = c.ContactType,
                Value = c.Value,
                Label = c.Label,
                EffectiveFrom = c.EffectiveFrom,
                EffectiveTo = c.EffectiveTo,
                IsCurrent = c.IsCurrent,
                ChildContactItems = c.ChildContactItems.Count(a => a.IsCurrent) > 0 ? c.ChildContactItems.Where(a => a.IsCurrent).Select(a => new ContactItemDto
                {
                    Value = a.Value,
                    Label = a.Label,
                    ContactType = a.ContactType
                }).ToList() : null
            }).ToList();
        }
        public async Task DeActiveContactProfileAsync(Guid ProfileId)
        {
            ContactProfile? profile = await _contactProfileRepository.GetByIdAsync(ProfileId);
            if (profile == null)
                throw new Exception("Can Not Found Profile!!!");
            profile.DeActive();
        }
        public async Task ExpireAllContactAsync(Guid ProfileId)
        {                       
            IEnumerable<ContactItem> items = await _contactItemRepository.GetAllAsync(queryOptions: queryOptions => queryOptions.Where(q => q.ContactProfileId == ProfileId && q.IsCurrent));
            foreach (var item in items)
            {
                await item.DoExpire();
            }
            
        }
    }
}
