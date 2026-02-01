using Core.Application.Abstractions;
using Core.Application.Abstractions.HR;
using Core.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using User.Application.Interfaces;
using User.Domain.Entities;
using User.Infrastructure.Data;

namespace User.Infrastructure.Services
{
    public class PersonService : IPersonInternalService, IPersonPublicService
    {
        private readonly IRepository<UserDbContext, Person, Guid> _personRepository;
        private readonly ISpecificationRepository< Person, Guid> _personSpecRepository;
        private readonly ILogger<PersonService> _logger;
        public PersonService(IRepository<UserDbContext, Person, Guid> personRepository, ILogger<PersonService> logger, ISpecificationRepository<UserDbContext, Person, Guid> personSpecRepository)
        {
            _personRepository = personRepository;
            _personSpecRepository = personSpecRepository;
            _logger = logger;
        }
      
    }
}
