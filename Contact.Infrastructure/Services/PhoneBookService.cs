using Core.Application.Abstractions;
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
using Core.Application.Abstractions.Contact;
using Contact.Infrastructure.Data;
using Core.Application.Abstractions.HR;
using Core.Application.Abstractions.People;

namespace Contact.Infrastructure.Services
{
    public class PhoneBookService : IPhoneBookInternalService, IPhoneBookPublicService
    {
        private readonly ISpecificationRepository<PhoneBookInfoView, Guid> _PhoneBookSpecRepository;
        private readonly ISpecificationRepository<ContactProfileAssignment, Guid> _assignmentSpecRepository;
        private readonly IRepository<ContactDbContext, ContactProfileAssignment, Guid> _assignmentRepository;
        private readonly IEmploymentPublicService _employmentservice;
        private readonly IPostPublicService _postservice;
        private readonly ILocationPublicService _locationservice;
        private readonly IPersonPublicService _personservice;
        private readonly ILogger<PhoneBookService> _logger;

        public PhoneBookService(ILogger<PhoneBookService> logger,
            ISpecificationRepository<PhoneBookInfoView, Guid> PhoneBookSpecRepository,
            IRepository<ContactDbContext, ContactProfileAssignment, Guid> assignmentRepository,
            ISpecificationRepository<ContactProfileAssignment, Guid> assignmentSpecRepository)
        {
            _PhoneBookSpecRepository = PhoneBookSpecRepository;
            _assignmentSpecRepository = assignmentSpecRepository;
            _assignmentRepository = assignmentRepository;
            _logger = logger;
        }

       
        public async Task<IReadOnlyList<PhoneBookEmploymentDto>> GetPhoneBookListAsync(Guid? organUnitId)
        {
            var contactList = await _assignmentRepository.GetAllAsync(queryOptions: q=>q.Where(a=>a.IsCurrent).Include(a=>a.ContactResource).Include(a=>a.ContactProfile) );
             
            IReadOnlyList<PhoneBookEmploymentDto> result = list.ToPhoneBookDtos();
            return result;
        }
        //public async Task<IReadOnlyList<PhoneBookEmploymentDto>> GetPhoneBookListAsync(Guid? organUnitId)
        //{
        //    GetPhoneBookSpec spec = new GetPhoneBookSpec();
        //    var list = await _PhoneBookSpecRepository.ListBySpecAsync(spec);
        //    IReadOnlyList<PhoneBookEmploymentDto> result = list.ToPhoneBookDtos();
        //    return result;
        //}
    }
}
