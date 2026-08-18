using Contact.Application.Interfaces;
using Contact.Domain.Entities;
using Contact.Domain.Specifications;
using Contact.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Domain.Common.EntityProperties;
using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
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
        private readonly IRepository<ContactDbContext, PartyContact, Guid> _personContactRepository;
        private readonly ISpecificationRepository<PartyContact, Guid> _personContactSpecRepository;

        private readonly IRepository<ContactDbContext, EmploymentContact, Guid> _employmentContactRepository;
        private readonly ISpecificationRepository<EmploymentContact, Guid> _employmentContactSpecRepository;

        private readonly IRepository<ContactDbContext, PostContact, Guid> _postContactRepository;
        private readonly ISpecificationRepository<PostContact, Guid> _postContactSpecRepository;

        private readonly IRepository<ContactDbContext, LocationContact, Guid> _locationContactRepository;
        private readonly ISpecificationRepository<LocationContact, Guid> _locationContactSpecRepository;

        private readonly IUnitOfWork<ContactDbContext> _uow;
        private readonly ILogger<ContactService> _logger;


        
        public ContactService(ILogger<ContactService> logger , 
            IRepository<ContactDbContext, PartyContact, Guid> personContactRepository,
            ISpecificationRepository<PartyContact, Guid> personContactSpecRepository,
        IRepository<ContactDbContext, EmploymentContact, Guid> employmentContactRepository,
            ISpecificationRepository<EmploymentContact, Guid> employmentContactSpecRepository,
        IRepository<ContactDbContext, LocationContact, Guid> locationContactRepository,
            ISpecificationRepository<LocationContact, Guid> locationContactSpecRepository,
            IRepository<ContactDbContext, PostContact, Guid> postContactRepository,
        ISpecificationRepository<PostContact, Guid> postContactSpecRepository,

        IUnitOfWork<ContactDbContext> uow)
        {
            _personContactRepository = personContactRepository;
            _personContactSpecRepository = personContactSpecRepository;
            _employmentContactRepository = employmentContactRepository;
            _employmentContactSpecRepository = employmentContactSpecRepository;
            _locationContactRepository = locationContactRepository;
            _locationContactSpecRepository = locationContactSpecRepository;
            _postContactRepository = postContactRepository;
            _postContactSpecRepository= postContactSpecRepository;

            _logger = logger;
            _uow = uow;
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }

        public async Task CreatePartyContact(PartyContactType type, List<string>? values, Guid partyId)
       {
            if (values != null)
            {
                // ۱. دریافت مکان‌های فعال فعلی کارمند (فرض بر این است که اسپک فقط Activeها را برمی‌گرداند)               
                GetPartyContactSpec spec = new GetPartyContactSpec(type, partyId, values);
                IEnumerable<PartyContact>? existContact = await _personContactSpecRepository.ListBySpecAsync(spec);

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
                    .Select(val => new PartyContact(type, val, partyId, DateTime.UtcNow))
                    .ToList();

                if (toAdd.Any())
                {
                    await _personContactRepository.AddRangeAsync(toAdd);
                }

            }
        }
       
        public async Task CreateEmploymentContact(HrContactType type, List<string>? values, Guid employmentId)
        {
            if (values != null)
            {
                // ۱. دریافت مکان‌های فعال فعلی کارمند (فرض بر این است که اسپک فقط Activeها را برمی‌گرداند)               
                GetEmploymentContactSpec spec = new GetEmploymentContactSpec(type, employmentId, values);
                IEnumerable<EmploymentContact>? existContact = await _employmentContactSpecRepository.ListBySpecAsync(spec);

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
                    .Select(val => new EmploymentContact(type, val, employmentId, DateTime.UtcNow))
                    .ToList();

                if (toAdd.Any())
                {
                    await _employmentContactRepository.AddRangeAsync(toAdd);
                }

            }
        }
        public async Task CreateLocationContact(HrContactType type, List<string>? values, Guid locationId)
        {
            if (values != null)
            {
                // ۱. دریافت مکان‌های فعال فعلی کارمند (فرض بر این است که اسپک فقط Activeها را برمی‌گرداند)               
                GetLocationContactSpec spec = new GetLocationContactSpec(type, locationId, values);
                IEnumerable<LocationContact>? existContact = await _locationContactSpecRepository.ListBySpecAsync(spec);

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
                    .Select(val => new LocationContact(type, val, locationId, DateTime.UtcNow))
                    .ToList();

                if (toAdd.Any())
                {
                    await _locationContactRepository.AddRangeAsync(toAdd);
                }

            }
        }
        
        public async Task CreatePostContact(HrContactType type, List<string>? values, Guid postId)
        {
            if (values != null)
            {
                // ۱. دریافت مکان‌های فعال فعلی کارمند (فرض بر این است که اسپک فقط Activeها را برمی‌گرداند)               
                GetPostContactSpec spec = new GetPostContactSpec(type, postId, values);
                IEnumerable<PostContact>? existContact = await _postContactSpecRepository.ListBySpecAsync(spec);

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
                    .Select(val => new PostContact(type, val, postId, DateTime.UtcNow))
                    .ToList();

                if (toAdd.Any())
                {
                    await _postContactRepository.AddRangeAsync(toAdd);
                }

            }
        }
        public async Task<List<EntityContactDto<HrContactType>>> GetLocationContactsByLocationIdsAsync(List<Guid> locationIds)
        {
            GetLocationContactsByLocationIdsSpec spec = new GetLocationContactsByLocationIdsSpec(locationIds);
            var list = await _locationContactSpecRepository.ListBySpecAsync(spec);
            return list.Select(c => new EntityContactDto<HrContactType>
            {
                ContactType = c.ContactType,
                Value = c.Value,
                EntityId = c.FkLocationId,
                IsCurrent = c.IsCurrent
            }).ToList();
        }
        public async Task<List<EntityContactDto<HrContactType>>> GetPostContactsByPostIdsAsync(List<Guid> postIds)
        {
            GetPostContactsByPostIdsSpec spec = new GetPostContactsByPostIdsSpec(postIds);
            var list = await _postContactSpecRepository.ListBySpecAsync(spec);
            return list.Select(c => new EntityContactDto<HrContactType>
            {
                ContactType = c.ContactType,
                Value = c.Value,
                EntityId = c.FkPostId,
                IsCurrent = c.IsCurrent
            }).ToList();
        }
        public async Task<List<EntityContactDto<HrContactType>>> GetEmploymentContactsByEmploymentIdsAsync(List<Guid> employmentIds)
        {
            GetEmploymentContactsByEmploymentIdsSpec spec = new GetEmploymentContactsByEmploymentIdsSpec(employmentIds);
            var list = await _employmentContactSpecRepository.ListBySpecAsync(spec);
            return list.Select(c => new EntityContactDto<HrContactType>
            {
                ContactType = c.ContactType,
                Value = c.Value,
                EntityId = c.FkEmploymentId,
                IsCurrent = c.IsCurrent
            }).ToList();
        }

        public async Task<List<EntityContactDto<PartyContactType>>> GetPartyContactsByPartyIdsAsync(List<Guid> partyIds)
        {
            GetPartyContactsByPartyIdsSpec spec = new GetPartyContactsByPartyIdsSpec(partyIds);
            var list = await _personContactSpecRepository.ListBySpecAsync(spec);
            return list.Select(c => new EntityContactDto<PartyContactType>
            {
                ContactType = c.ContactType,
                Value = c.Value,
                EntityId = c.FkPartyId,
                IsCurrent = c.IsCurrent
            }).ToList();
        }
    }
}
