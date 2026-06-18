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

namespace People.Infrastructure.Services
{
    public class PersonService : IPersonInternalService, IPersonPublicService
    {
        private readonly IRepository<PeopleDbContext, naturalPerson, Guid> _personRepository;
        private readonly ISpecificationRepository< naturalPerson, Guid> _personSpecRepository;
        private readonly ILogger<PersonService> _logger;
        private readonly IUnitOfWork<PeopleDbContext> _uow;

        public PersonService(IRepository<PeopleDbContext, naturalPerson, Guid> personRepository, ILogger<PersonService> logger, 
            ISpecificationRepository< naturalPerson, Guid> personSpecRepository,
            IUnitOfWork<PeopleDbContext> uow)
        {
            _personRepository = personRepository;
            _personSpecRepository = personSpecRepository;
            _logger = logger;
            _uow = uow;
        }

        public async Task<Guid> CreatePersonAsync(string nationalCode, string firstName, string lastName, DateTime? birthDate, string? birthPlace, string? fatherName, Gender? gender)
        {
            naturalPerson person = new naturalPerson(nationalCode, firstName, lastName, birthDate, birthPlace);
            await _personRepository.AddAsync(person);
            return person.Id;
        }

        public async Task SaveAsync()
        {
            await _uow.SaveChangesAsync();
        }
    }
}
