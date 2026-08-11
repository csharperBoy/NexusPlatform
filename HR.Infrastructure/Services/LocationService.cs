using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.People;
using Core.Domain.Common.EntityProperties;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
using HR.Application.DTOs;
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
    public class LocationService : ILocationInternalService, ILocationPublicService
    {

        private readonly IRepository<HRDbContext, Location, Guid> _LocationRepository;
        private readonly IRepository<HRDbContext, LocationContact, Guid> _LocationContactRepository;
        private readonly IRepository<HRDbContext, PostLocation, Guid> _PostLocationsRepository;
        private readonly IRepository<HRDbContext, EmploymentLocation, Guid> _EmploymentLocationsRepository;
        private readonly ISpecificationRepository<Location, Guid> _LocationSpecRepository;
        private readonly ISpecificationRepository<PostLocation, Guid> _PostLocationSpecRepository;
        private readonly ISpecificationRepository<EmploymentLocation, Guid> _EmploymentLocationSpecRepository;
        private readonly ISpecificationRepository<LocationContact, Guid> _LocationContactSpecRepository;
        private readonly ILogger<LocationService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;

        public LocationService(
            IRepository<HRDbContext, Location, Guid> LocationRepository,
        IRepository<HRDbContext, LocationContact, Guid> LocationContactRepository,
        IRepository<HRDbContext, PostLocation, Guid> PostLocationsRepository,
        IRepository<HRDbContext, EmploymentLocation, Guid> EmploymentLocationsRepository,
        ISpecificationRepository<Location, Guid> LocationSpecRepository,
        ISpecificationRepository<PostLocation, Guid> PostLocationSpecRepository,
        ISpecificationRepository<EmploymentLocation, Guid> EmploymentLocationSpecRepository,
        ISpecificationRepository<LocationContact, Guid> LocationContactSpecRepository,
        ILogger<LocationService> logger,
        IUnitOfWork<HRDbContext> uow


            )
        {
            _LocationRepository = LocationRepository;
            _LocationContactRepository = LocationContactRepository;
            _PostLocationsRepository = PostLocationsRepository;
            _EmploymentLocationsRepository = EmploymentLocationsRepository;
            _LocationSpecRepository = LocationSpecRepository;
            _PostLocationSpecRepository = PostLocationSpecRepository;
            _EmploymentLocationSpecRepository = EmploymentLocationSpecRepository;
            _LocationContactSpecRepository = LocationContactSpecRepository;
            _logger = logger;
            _uow = uow;
        }

        public async Task<Guid> CreateLocationAsync(
          string _title,

        string? _orgPhone = null,
        Email? _orgEmail = null,
        string? _orgMobile = null
            )
        {

            Location loc = new Location(_title);
            await _LocationRepository.AddAsync(loc);

            await CreateLocationContact(HrContactType.OrgMobile, _orgMobile, loc.Id);
            await CreateLocationContact(HrContactType.OfficePhone, _orgPhone, loc.Id);
            await CreateLocationContact(HrContactType.OrgEmail, _orgEmail?.Value, loc.Id);
            return loc.Id;
        }
        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }
      /*  public async Task<Guid?> GetLocationId(Guid? personId)
        {
            GetLocationByPersonIdSpec spec = new GetLocationByPersonIdSpec(personId);
            Location? Location = await _LocationSpecRepository.GetBySpecAsync(spec);
            if (Location == null)
                //throw new InvalidOperationException("Location not found!!!");
                return null;

            return Location.Id;

        }*/

      /*  public async Task AssignLocationsToLocation(Guid LocationId, List<Guid> locationsId)
        {
            // ۱. دریافت مکان‌های فعال فعلی کارمند (فرض بر این است که اسپک فقط Activeها را برمی‌گرداند)
            var spec = new GetLocationLocationsSpec(LocationId);
            var existingActive = await _LocationLocationSpecRepository.ListBySpecAsync(spec);

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
                .Select(id => new LocationLocation(id, LocationId))
                .ToList();

            if (toAdd.Any())
            {
                await _LocationLocationsRepository.AddRangeAsync(toAdd);
            }

        }*/



       public async Task<Guid> UpdateLocationAsync(Guid id, string? title, string? officePhone, string? orgEmail, string? orgMobile)
        {
            Location? loc = await _LocationRepository.GetByIdAsync(id);
            if (loc == null)
                throw new Exception("can not found Location!!!");

            bool hasChange = loc.ApplyChange(title);
            if (hasChange)
            {
                await _LocationRepository.UpdateAsync(loc);
            }

            if (officePhone != null)
            {
                await CreateLocationContact(HrContactType.OfficePhone, officePhone, loc.Id);
            }
            if (orgEmail != null)
            {
                await CreateLocationContact(HrContactType.OrgEmail, orgEmail, loc.Id);
            }
            if (orgMobile != null)
            {
                await CreateLocationContact(HrContactType.OrgMobile, orgMobile, loc.Id);
            }
            return loc.Id;
        }
        private async Task CreateLocationContact(HrContactType type, string? value, Guid LocationId)
        {
            if (value != null)
            {
                GetLocationContactSpec spec = new GetLocationContactSpec(type, LocationId, value);
                LocationContact? existContact = await _LocationContactSpecRepository.GetBySpecAsync(spec);
                if (existContact?.Value.Trim() != value.Trim())
                {
                    if (existContact != null)
                    {
                        await existContact.DoExpire();
                        await _LocationContactRepository.UpdateAsync(existContact);

                    }
                    LocationContact contact = new LocationContact(type, value, LocationId, DateTime.UtcNow);
                    await _LocationContactRepository.AddAsync(contact);
                }

            }
        }

        public async Task<IReadOnlyList<LocationInfoDto>> GetLocationListAsync()
        {
            var list = await _LocationRepository.GetAllAsync(i=> i.LocationContacts);
            return list.Select(s=>new LocationInfoDto
            {
                Id = s.Id,
                Title = s.Title,
                orgMobile = s.LocationContacts.FirstOrDefault(c => c.ContactType == HrContactType.OrgMobile && c.IsCurrent)?.Value,
                orgPhone = s.LocationContacts.FirstOrDefault(c => c.ContactType == HrContactType.OfficePhone && c.IsCurrent)?.Value
            }).ToList();
        }
    }
}
