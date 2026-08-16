using Contact.Application.Interfaces;
using Contact.Domain.Entities;
using Contact.Domain.Specifications;
using Contact.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Domain.Common.EntityProperties;
using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
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

        private readonly IRepository<ContactDbContext, EmploymentContact, Guid> _employmentContactRepository;
        private readonly ISpecificationRepository<EmploymentContact, Guid> _employmentContactSpecRepository;

        private readonly IRepository<ContactDbContext, PostContact, Guid> _postContactRepository;
        private readonly ISpecificationRepository<PostContact, Guid> _postContactSpecRepository;

        private readonly IRepository<ContactDbContext, LocationContact, Guid> _locationContactRepository;
        private readonly ISpecificationRepository<LocationContact, Guid> _locationContactSpecRepository;
        private readonly IUnitOfWork<ContactDbContext> _uow;
        private readonly ILogger<ContactService> _logger;


        
        public ContactService(ILogger<ContactService> logger , IRepository<ContactDbContext, PartyContact, Guid> personContactRepository,
        IRepository<ContactDbContext, EmploymentContact, Guid> employmentContactRepository,
            ISpecificationRepository<EmploymentContact, Guid> employmentContactSpecRepository,
        IRepository<ContactDbContext, LocationContact, Guid> locationContactRepository,
            ISpecificationRepository<LocationContact, Guid> locationContactSpecRepository,
            IRepository<ContactDbContext, PostContact, Guid> postContactRepository,
        ISpecificationRepository<PostContact, Guid> postContactSpecRepository,

        IUnitOfWork<ContactDbContext> uow)
        {
            _personContactRepository = personContactRepository;
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

        public async Task CreatePartyContact(PartyContactType type, string? value, Guid partyId)
       {
           if (value != null)
           {
               PartyContact contact = new PartyContact(type, value, partyId);
               await _personContactRepository.AddAsync(contact);
           }
       }
        public async Task CreateEmploymentContact(HrContactType type, string? value, Guid employmentId)
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
        public async Task CreateLocationContact(HrContactType type, string? value, Guid LocationId)
        {
            if (value != null)
            {
                GetLocationContactSpec spec = new GetLocationContactSpec(type, LocationId, value);
                LocationContact? existContact = await _locationContactSpecRepository.GetBySpecAsync(spec);
                if (existContact?.Value.Trim() != value.Trim())
                {
                    if (existContact != null)
                    {
                        await existContact.DoExpire();
                        await _locationContactRepository.UpdateAsync(existContact);

                    }
                    LocationContact contact = new LocationContact(type, value, LocationId, DateTime.UtcNow);
                    await _locationContactRepository.AddAsync(contact);
                }

            }
        }
        
        public async Task CreatePostContact(HrContactType type, string? value, Guid postId)
        {
            if (value != null)
            {
                GetPostContactSpec spec = new GetPostContactSpec(type, postId, value);
                PostContact? existContact = await _postContactSpecRepository.GetBySpecAsync(spec);
                if (existContact?.Value.Trim() != value.Trim())
                {
                    if (existContact != null)
                    {
                       await existContact.DoExpire();
                        await _postContactRepository.UpdateAsync(existContact);

                    }
                    PostContact contact = new PostContact(type, value, postId, DateTime.UtcNow);
                    await _postContactRepository.AddAsync(contact);
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
    }
}
