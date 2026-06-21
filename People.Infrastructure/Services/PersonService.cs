using Core.Application.Abstractions;
using Core.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using People.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using People.Application.Interfaces;
using People.Domain.Entities;
using Core.Application.Abstractions.People;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
using Core.Application.Provider;

namespace People.Infrastructure.Services
{
    public class PersonService : IPersonInternalService, IPersonPublicService
    {
        private readonly IUserDataContextProvider _userProvider;
        private readonly IRepository<PeopleDbContext, naturalPersons, Guid> _naturalPersonRepository;
        private readonly IRepository<PeopleDbContext, Parties, Guid> _partyRepository;
        private readonly ISpecificationRepository< naturalPersons, Guid> _personSpecRepository;
        private readonly ILogger<PersonService> _logger;
        private readonly IUnitOfWork<PeopleDbContext> _uow;

        public PersonService(IRepository<PeopleDbContext, naturalPersons, Guid> naturalPersonRepository,
            IUserDataContextProvider userProvider,
            ILogger<PersonService> logger, 
            ISpecificationRepository< naturalPersons, Guid> personSpecRepository,
            IRepository<PeopleDbContext, Parties, Guid> partyRepository,
            IUnitOfWork<PeopleDbContext> uow)
        {
            _naturalPersonRepository = naturalPersonRepository;
            _personSpecRepository = personSpecRepository;
            _partyRepository = partyRepository;
            _userProvider = userProvider;
            _logger = logger;
            _uow = uow;
        }

        public async Task<Guid> CreatePersonAsync(string nationalCode, string firstName, string lastName, DateTime? birthDate, string? birthPlace, string? fatherName, Gender? gender)
        {
            var user =await _userProvider.GetAsync(new CancellationToken());
           naturalPersons naturalPerson = new naturalPersons(nationalCode, firstName, lastName, birthDate, birthPlace,fatherName,gender, user.UserName);            
            naturalPerson.setParty(await CreatePartyAsync());
            await _naturalPersonRepository.AddAsync(naturalPerson);
            return naturalPerson.Id;
        }
        private async Task<Guid> CreatePartyAsync()
        {
            Parties party = new Parties();
            
            await _partyRepository.AddAsync(party);
            return party.Id;
        }
        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }
    }
}
