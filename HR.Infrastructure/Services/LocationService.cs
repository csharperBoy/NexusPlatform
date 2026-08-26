using Core.Application.Abstractions;
using Core.Application.Abstractions.Contact;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.People;
using Core.Domain.Common.EntityProperties;
using Core.Domain.ValueObjects;
using Core.Shared.DTOs.HR;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using HR.Domain.Events.Location;
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
        private readonly IRepository<HRDbContext, PostLocation, Guid> _PostLocationsRepository;
        private readonly IRepository<HRDbContext, EmploymentLocation, Guid> _employmentLocationsRepository;
        private readonly ISpecificationRepository<Location, Guid> _LocationSpecRepository;
        private readonly ISpecificationRepository<PostLocation, Guid> _PostLocationSpecRepository;
        private readonly ISpecificationRepository<EmploymentLocation, Guid> _EmploymentLocationSpecRepository;
        private readonly IContactPublicService _contactService;
        private readonly ILogger<LocationService> _logger;
        private readonly IUnitOfWork<HRDbContext> _uow;

        public LocationService(
            IRepository<HRDbContext, Location, Guid> LocationRepository,
        IRepository<HRDbContext, PostLocation, Guid> PostLocationsRepository,
        IRepository<HRDbContext, EmploymentLocation, Guid> employmentLocationsRepository,
        ISpecificationRepository<Location, Guid> LocationSpecRepository,
        ISpecificationRepository<PostLocation, Guid> PostLocationSpecRepository,
        ISpecificationRepository<EmploymentLocation, Guid> EmploymentLocationSpecRepository,
       IContactPublicService contactService,
        ILogger<LocationService> logger,
        IUnitOfWork<HRDbContext> uow


            )
        {
            _LocationRepository = LocationRepository;
            _PostLocationsRepository = PostLocationsRepository;
            _employmentLocationsRepository = employmentLocationsRepository;
            _LocationSpecRepository = LocationSpecRepository;
            _PostLocationSpecRepository = PostLocationSpecRepository;
            _EmploymentLocationSpecRepository = EmploymentLocationSpecRepository;
            _contactService = contactService;
            _logger = logger;
            _uow = uow;
        }

        public async Task<Guid> CreateLocationAsync(
          string _title,

        List<string>? _orgPhone = null,
        List<string>? _orgEmail = null,
        List<string>? _orgMobile = null
            )
        {

            Guid contactProfileId = await _contactService.CreateContactProfileAsync($"Location - {_title}", ContactProfileTypeEnum.Location);
            Location loc = new Location(_title, contactProfileId);
            await _LocationRepository.AddAsync(loc);

            await _contactService.SyncProfileContacts(ContactTypeEnum.OrganizationMobile, _orgMobile, loc.FkContactProfileId);
            await _contactService.SyncProfileContacts(ContactTypeEnum.OfficePhone, _orgPhone, loc.FkContactProfileId);
            await _contactService.SyncProfileContacts(ContactTypeEnum.Email, _orgEmail, loc.FkContactProfileId);
            return loc.Id;
        }
        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
            await _contactService.SaveAsync();
        }
        


        public async Task<Guid> UpdateLocationAsync(Guid id, string? title, List<string>? officePhone, List<string>? orgEmail, List<string>? orgMobile)
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
                await _contactService.SyncProfileContacts(ContactTypeEnum.OfficePhone, officePhone, loc.FkContactProfileId);
            }
            if (orgEmail != null)
            {
                await _contactService.SyncProfileContacts(ContactTypeEnum.Email, orgEmail, loc.FkContactProfileId);
            }
            if (orgMobile != null)
            {
                await _contactService.SyncProfileContacts(ContactTypeEnum.OrganizationMobile, orgMobile, loc.FkContactProfileId);
            }
            return loc.Id;
        }

        public async Task<IReadOnlyList<LocationInfoDto>> GetLocationListAsync()
        {
            // ۱. دریافت تمام مکان‌ها از دیتابیس HR
            var locations = await _LocationRepository.GetAllAsync(queryOptions: query=> query.Where(q=>q.IsRemove != true));

            if (!locations.Any())
                return Array.Empty<LocationInfoDto>();

            // ۵. مپ کردن داده‌ها در حافظه
            return locations.Select(s =>
             new LocationInfoDto
             {
                 Id = s.Id,
                 Title = s.Title,
                 ProfileId = s.FkContactProfileId

             }).ToList();
        }

        public async Task DeleteAsync(Guid id)
        {
            Location? model = await _LocationRepository.GetByIdAsync(id);
            if (model == null)
                throw new Exception("can not found location!!!");

            await model.SoftRemove();
            model.AddDomainEvent(new RemoveLocationEvent(model.Id,model.FkContactProfileId,model.Title));
            
            await ExpireLocationPostsAsync(id);

            await ExpireLocationEmploymentsAsync(id);

        }

        private async Task ExpireLocationPostsAsync(Guid id)
        {
            var postList = await _PostLocationsRepository.GetAllAsync(queryOptions: q => q.Where(a => a.FkLocationId == id && a.IsCurrent));
            foreach (var item in postList)
            {
                item.DoExpire();
            }
        }
        private async Task ExpireLocationEmploymentsAsync(Guid id)
        {
            var locList = await _employmentLocationsRepository.GetAllAsync(queryOptions: q => q.Where(a => a.FkLocationId == id && a.IsCurrent));
            foreach (var item in locList)
            {
                item.DoExpire();
            }
        }

    }
}
