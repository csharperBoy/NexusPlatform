using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.Contact;
using Core.Application.Abstractions.People;
using Core.Application.Provider;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Repositories;
using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;

using Microsoft.Extensions.Logging;
using People.Application.Interfaces;
using People.Domain.Entities;
using People.Domain.Enums;
using People.Domain.Events;
using People.Domain.Specifications;
using People.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Infrastructure.Services
{
    public class PersonService : IPersonInternalService, IPersonPublicService
    {
        //private readonly IUserDataContextProvider _userProvider;

        private readonly IPermissionPublicService _permissionService;
        private readonly IRepository<PeopleDbContext, NaturalPerson, Guid> _naturalPersonRepository;
        private readonly IRepository<PeopleDbContext, NaturalPersonProfile, Guid> _naturalPersonProfileRepository;
        private readonly IRepository<PeopleDbContext, Party, Guid> _partyRepository;
        private readonly IContactPublicService _contactService;
        private readonly ISpecificationRepository<NaturalPerson, Guid> _personSpecRepository;
        private readonly ILogger<PersonService> _logger;
        private readonly IUnitOfWork<PeopleDbContext> _uow;

        public PersonService(IRepository<PeopleDbContext, NaturalPerson, Guid> naturalPersonRepository,
            IRepository<PeopleDbContext, NaturalPersonProfile, Guid> naturalPersonProfileRepository,
            //IUserDataContextProvider userProvider,
            IPermissionPublicService permissionService,
            ILogger<PersonService> logger,
            ISpecificationRepository<NaturalPerson, Guid> personSpecRepository,
            IRepository<PeopleDbContext, Party, Guid> partyRepository,
            IContactPublicService contactService,
            IUnitOfWork<PeopleDbContext> uow)
        {
            _naturalPersonRepository = naturalPersonRepository;
            _naturalPersonProfileRepository = naturalPersonProfileRepository;
            _personSpecRepository = personSpecRepository;
            _partyRepository = partyRepository;
            _contactService = contactService;
            //_userProvider = userProvider;
            _permissionService = permissionService;
            _logger = logger;
            _uow = uow;
        }

        public async Task<Guid> CreatePersonAsync(string nationalCode, string firstName, string lastName,
            DateTime? birthDate = null,
            string? birthPlace = null,
            string? fatherName = null,
            Gender? gender = null,
             List<PhoneNumber>? Phone = null,
       List<string>? Address = null,
        List<Email>? Email = null,
        List<PhoneNumber>? Mobile = null,
        string? createBy = null
            )
        {
            NaturalPerson? existPerson = (await _naturalPersonRepository.GetAllAsync(queryOptions: q => q.Where(a => a.NationalCode.Value.Trim() == nationalCode.Trim()))).FirstOrDefault();
            NaturalPerson naturalPerson = new NaturalPerson(nationalCode, firstName, lastName, birthDate, birthPlace, fatherName, gender, createBy);
            if (existPerson == null)
            {
                naturalPerson.setParty(await CreatePartyAsync(Phone, Address, Email, Mobile));
                await _naturalPersonRepository.AddAsync(naturalPerson);
                await _naturalPersonProfileRepository.AddAsync(new NaturalPersonProfile(naturalPerson.Id));
                return naturalPerson.Id;
            }
          else
          {
              existPerson.ApplyChange(naturalPerson,
                  new List<string> {
                  "NaturalPerson.NationalCode",
                  "NaturalPerson.FullName",
                  "NaturalPerson.BirthDate",
                  "NaturalPerson.BirthPlace",
                  "NaturalPerson.FatherName",
                  "NaturalPerson.Gender",
                  "NaturalPerson.CreatedBy"
              });
              await _naturalPersonRepository.UpdateAsync(existPerson);
              return naturalPerson.Id;
            }
        }
        private async Task<Guid> CreatePartyAsync(
             List<PhoneNumber>? Phone,
             List<string>? Address,
             List<Email>? Email,
             List<PhoneNumber>? Mobile)
        {
            Guid perAssigneeId = await _permissionService.CreatePermissionAssigneeAsync(AssigneeType.Party);
            Guid contactProfileId = await _contactService.CreateContactProfileAsync($"Party - {perAssigneeId}", ContactProfileTypeEnum.Party);
            Party party = new Party(contactProfileId, contactProfileId);
            await _partyRepository.AddAsync(party);

            await _contactService.SyncProfileContacts(ContactTypeEnum.Mobile, Mobile?.Select(a => a.Value).ToList(), party.FkContactProfileId);
            await _contactService.SyncProfileContacts(ContactTypeEnum.Phone, Phone?.Select(a => a.Value).ToList(), party.FkContactProfileId);
            await _contactService.SyncProfileContacts(ContactTypeEnum.Address, Address, party.FkContactProfileId);
            await _contactService.SyncProfileContacts(ContactTypeEnum.Email, Email?.Select(a => a.Value).ToList(), party.FkContactProfileId);
            return party.Id;
        }



        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
            await _contactService.SaveAsync();
        }

        public async Task<Guid?> GetPersonPermissionAssigneeIdAsync(Guid? personId)
        {
            var person = await _naturalPersonRepository.GetByIdAsync(personId ?? Guid.Empty, a => a.Party);
            return person?.Party.FkPermissionAssigneeId;
        }
        public async Task<Guid?> GetPartyPermissionAssigneeIdAsync(Guid? partyId)
        {
            var party = await _partyRepository.GetByIdAsync(partyId ?? Guid.Empty);
            return party?.FkPermissionAssigneeId;
        }

        public async Task<Guid?> GetNaturalPersonIdAsync(Guid? partyId)
        {
            GetNaturalPersonByPartyId spec = new GetNaturalPersonByPartyId(partyId);
            var person = await _personSpecRepository.GetBySpecAsync(spec);
            return person?.Id;

        }

        public async Task UpdatePersonAsync(Guid id,
            Optional<string> firstlName,
            Optional<string> lastName,
            Optional<DateTime?> birthDate,
            Optional<string?> birthPlace,
            Optional<string?> fatherName,
            Optional<string?> nationalCode,
            Optional<List<PhoneNumber>?> Phone,
            Optional<List<string>?> Address,
            Optional<List<Email>?> Email,
            Optional<List<PhoneNumber>?> Mobile
            )
        {
            NaturalPerson? person = await _naturalPersonRepository.GetByIdAsync(id, a => a.Party);
            if (person == null)
            {
                throw new Exception("Person not found");
            }

            bool hasChange = await person.ApplyChange(
                nationalCode,
                firstlName,
                lastName,
                birthDate,
                birthPlace,
                fatherName,
                null
                );
            //bool hasChange = person.ApplyChange( new NaturalPerson(
            //    nationalCode,
            //    firstlName,
            //    lastName,
            //    birthDate, 
            //    birthPlace, 
            //    fatherName,
            //    null,null
            //    ) ,  UpdateMask);

            if (hasChange)
            {
                await _naturalPersonRepository.UpdateAsync(person);
            }

            if (Mobile.IsSet)
                await _contactService.SyncProfileContacts(ContactTypeEnum.Mobile, Mobile.Value?.Select(a => a.Value).ToList(), person.Party.FkContactProfileId);
            if (Phone.IsSet)
                await _contactService.SyncProfileContacts(ContactTypeEnum.Phone, Phone.Value?.Select(a => a.Value).ToList(), person.Party.FkContactProfileId);
            if (Address.IsSet)
                await _contactService.SyncProfileContacts(ContactTypeEnum.Address, Address.Value, person.Party.FkContactProfileId);
            if (Email.IsSet)
                await _contactService.SyncProfileContacts(ContactTypeEnum.Email, Email.Value?.Select(a => a.Value).ToList(), person.Party.FkContactProfileId);

            person.AddDomainEvent(new ChangeNaturalPersonEvent(person.Id));
        }
    }
}
