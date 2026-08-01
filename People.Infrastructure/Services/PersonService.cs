using Core.Application.Abstractions;
using Core.Application.Abstractions.Authorization.PublicService;
using Core.Application.Abstractions.People;
using Core.Application.Provider;
using Core.Domain.ValueObjects;
using Core.Infrastructure.Repositories;
using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using Microsoft.Extensions.Logging;
using People.Application.Interfaces;
using People.Domain.Entities;
using People.Domain.Enums;
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
        private readonly IRepository<PeopleDbContext, PartyContact, Guid> _personContactRepository;
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
            IRepository<PeopleDbContext, PartyContact, Guid> personContactRepository,
            IUnitOfWork<PeopleDbContext> uow)
        {
            _naturalPersonRepository = naturalPersonRepository;
            _naturalPersonProfileRepository = naturalPersonProfileRepository;
            _personSpecRepository = personSpecRepository;
            _partyRepository = partyRepository;
            _personContactRepository = personContactRepository;
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
             PhoneNumber? Phone = null,
        string? Address = null,
        Email? Email = null,
        PhoneNumber? Mobile = null,
        string? createBy = null
            )
        {
            //var user =await _userProvider.GetAsync(new CancellationToken());
            NaturalPerson naturalPerson = new NaturalPerson(nationalCode, firstName, lastName, birthDate, birthPlace, fatherName, gender, createBy);
            naturalPerson.setParty(await CreatePartyAsync(Phone, Address, Email, Mobile));
            await _naturalPersonRepository.AddAsync(naturalPerson);
            await _naturalPersonProfileRepository.AddAsync(new NaturalPersonProfile(naturalPerson.Id));
            return naturalPerson.Id;
        }
        private async Task<Guid> CreatePartyAsync(
             PhoneNumber? Phone,
        string? Address,
        Email? Email,
        PhoneNumber? Mobile)
        {
            Guid perAssigneeId = await _permissionService.CreatePermissionAssigneeAsync(AssigneeType.Party);
            Party party = new Party(perAssigneeId);
            await _partyRepository.AddAsync(party);

            await CreatePartyContact(PartyContactType.Mobile, Mobile?.Value, party.Id);
            await CreatePartyContact(PartyContactType.Phone, Phone?.Value, party.Id);
            await CreatePartyContact(PartyContactType.Address, Address, party.Id);
            await CreatePartyContact(PartyContactType.Email, Email?.Value, party.Id);
            return party.Id;
        }

        private async Task CreatePartyContact(PartyContactType type, string? value, Guid partyId)
        {
            if (value != null)
            {
                PartyContact contact = new PartyContact(type, value, partyId);
                await _personContactRepository.AddAsync(contact);
            }
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
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

        public async Task UpdatePersonAsync(Guid id,  string? phone, string? address, string? email, string? mobile, string firstlName, string lastName, DateTime? birthDate, string? birthPlace, string? fatherName)
        {
            NaturalPerson? person = await _naturalPersonRepository.GetByIdAsync(id, a => a.Party);
            if (person == null)
            {
                throw new Exception("Person not found");
            }

            bool hasChange = await person.ApplyChange(
                null,
                firstlName,
                lastName,
                birthDate, 
                birthPlace, 
                fatherName,
                null, 
                phone != null ? PhoneNumber.Create(phone) : null,
                mobile != null ? PhoneNumber.Create(mobile) : null,
                address, 
                email != null ? Email.Create(email) : null);

            if (hasChange) {
                await _naturalPersonRepository.UpdateAsync(person);
            }
        }
    }
}
