using Contact.Application.DTOs;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.Queries
{
    
    public record GetEmploymentContactListQuery(string? employmentCode = null)
   : IRequest<Result<IReadOnlyList<EmploymentContactDto>>>;

    public class GetEmploymentContactListQueryHandler
        : IRequestHandler<GetEmploymentContactListQuery, Result<IReadOnlyList<EmploymentContactDto>>>
    {
        private readonly IEmploymentInternalService _employmentInternalService;
        private readonly ILogger<GetEmploymentContactListQueryHandler> _logger;

        public GetEmploymentContactListQueryHandler(
            IEmploymentInternalService employmentInternalService,
        ILogger<GetEmploymentContactListQueryHandler> logger)
        {
            _employmentInternalService = employmentInternalService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<EmploymentContactDto>>> Handle(
            GetEmploymentContactListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting EmploymentContact List:");

                var employments = await _employmentInternalService.GetEmploymentListAsync();
                IReadOnlyList<EmploymentContactDto> result = employments
                    .Where(e => string.IsNullOrEmpty(request.employmentCode) || e.EmploymentCode == request.employmentCode)
                    .Select(e => new EmploymentContactDto
                    {
                        Id = e.Id,
                        NationalCode = e.NationalCode,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        EmploymentCode = e.EmploymentCode,
                        PartyMobile = e.partyContacts.FirstOrDefault(a => a.ContactType == PartyContactType.Mobile)?.Value
                        ,
                        PartyAddress = e.partyContacts.FirstOrDefault(a =>  a.ContactType == PartyContactType.Address)?.Value,
                        PartyPhone = e.partyContacts.FirstOrDefault(a =>  a.ContactType == PartyContactType.Phone)?.Value,
                        PartyEmail = e.partyContacts.FirstOrDefault(a =>  a.ContactType == PartyContactType.Email)?.Value,
                     
                        EmploymentContactPhone = e.Contacts.FirstOrDefault(a =>  a.ContactType == HrContactType.OfficePhone)?.Value,
                        EmploymentContactMobile = e.Contacts.FirstOrDefault(a => a.ContactType == HrContactType.OrgMobile)?.Value,
                        EmploymentContactEmail = e.Contacts.FirstOrDefault(a => a.ContactType == HrContactType.OrgEmail)?.Value,
                        EmploymentContactFax = e.Contacts.FirstOrDefault(a =>  a.ContactType == HrContactType.Fax)?.Value,
                    })
                    .ToList();
                return Result<IReadOnlyList<EmploymentContactDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get EmploymentContact List");
                return Result<IReadOnlyList<EmploymentContactDto>>.Fail(ex.Message);
            }
        }
    }
}
