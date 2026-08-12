using Core.Application.Abstractions;
using Core.Application.Abstractions.PhoneBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Contact.Application.DTOs;
using Contact.Application.Interfaces;
using Contact.Application.Mapping;
using Contact.Domain.Entities;
using Contact.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Contact.Infrastructure.Services
{
    public class PhoneBookService : IPhoneBookInternalService, IPhoneBookPublicService
    {
        private readonly ISpecificationRepository<PhoneBookInfoView, Guid> _PhoneBookSpecRepository;
        private readonly ILogger<PhoneBookService> _logger;

        public PhoneBookService(ILogger<PhoneBookService> logger,
            ISpecificationRepository<PhoneBookInfoView, Guid> PhoneBookSpecRepository)
        {
            _PhoneBookSpecRepository = PhoneBookSpecRepository;
            _logger = logger;
        }

       
        public async Task<IReadOnlyList<PhoneBookEmploymentDto>> GetPhoneBookListAsync(Guid? organUnitId)
        {
            GetPhoneBookSpec spec = new GetPhoneBookSpec();
            var list = await _PhoneBookSpecRepository.ListBySpecAsync(spec);
            IReadOnlyList<PhoneBookEmploymentDto> result = list.ToPhoneBookDtos();
            return result;
        }
    }
}
