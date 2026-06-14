using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
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

namespace People.Infrastructure.Services
{
    public class PersonService : IPersonInternalService, IPersonPublicService
    {
        private readonly IRepository<PersonDbContext, Person, Guid> _personRepository;
        private readonly ISpecificationRepository< Person, Guid> _personSpecRepository;
        private readonly ILogger<PersonService> _logger;
        public PersonService(IRepository<PersonDbContext, Person, Guid> personRepository, ILogger<PersonService> logger, ISpecificationRepository< Person, Guid> personSpecRepository)
        {
            _personRepository = personRepository;
            _personSpecRepository = personSpecRepository;
            _logger = logger;
        }
      
    }
}
