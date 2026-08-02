using Core.Application.Abstractions;
using Core.Application.Abstractions.PhoneBook;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhoneBook.Application.DTOs;
using PhoneBook.Application.Interfaces;
using PhoneBook.Application.Mapping;
using PhoneBook.Domain.Entities;
using PhoneBook.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhoneBook.Infrastructure.Services
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
