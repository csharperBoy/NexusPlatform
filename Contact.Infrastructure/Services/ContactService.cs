using Contact.Application.Interfaces;
using Contact.Domain.Entities;
using Contact.Infrastructure.Data;
using Core.Application.Abstractions;
using Core.Shared.Enums.People;
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

        private readonly IUnitOfWork<ContactDbContext> _uow;
        private readonly ILogger<ContactService> _logger;
        public ContactService(ILogger<ContactService> logger , IRepository<ContactDbContext, PartyContact, Guid> personContactRepository, IUnitOfWork<ContactDbContext> uow)
        {
            _personContactRepository = personContactRepository;
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
    }
}
