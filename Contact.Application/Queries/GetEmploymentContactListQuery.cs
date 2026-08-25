using Contact.Application.DTOs;
using Core.Application.Abstractions.Contact;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
 
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
        private readonly IContactPublicService _ContactService;
        public GetEmploymentContactListQueryHandler(
            IEmploymentInternalService employmentInternalService,
            IContactPublicService ContactService,


        ILogger<GetEmploymentContactListQueryHandler> logger)
        {
            _employmentInternalService = employmentInternalService;
            _ContactService = ContactService;
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
                var emptProfIds = employments.Where(p=>p.ProfileId!=null).Select(p => (Guid)p.ProfileId).ToList();
                var partyProfIds = employments.Select(p => p.PartyId).ToList();

                var empContactList = await _ContactService.GetContactsByProfilesIdsAsync(emptProfIds);
                var partyContactList = await _ContactService.GetContactsByProfilesIdsAsync(partyProfIds);

                IReadOnlyList<EmploymentContactDto> result = employments
                    .Where(e => string.IsNullOrEmpty(request.employmentCode) || e.EmploymentCode == request.employmentCode)
                    .Select(e => new EmploymentContactDto
                    {
                        Id = e.Id,
                        NationalCode = e.NationalCode,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        EmploymentCode = e.EmploymentCode,
                        PartyMobile = partyContactList.Where(a =>a.ProfileId == e.PartyProfileId && a.ContactType == ContactTypeEnum.Mobile && a.IsCurrent)?.Select(a=>a.Value).ToList(),
                        PartyAddress = partyContactList.Where(a => a.ProfileId == e.PartyProfileId && a.ContactType == ContactTypeEnum.Address && a.IsCurrent)?.Select(a => a.Value).ToList(),
                        PartyPhone = partyContactList.Where(a => a.ProfileId == e.PartyProfileId && a.ContactType == ContactTypeEnum.Phone && a.IsCurrent)?.Select(a => a.Value).ToList(),
                        PartyEmail = partyContactList.Where(a => a.ProfileId == e.PartyProfileId && a.ContactType == ContactTypeEnum.Email && a.IsCurrent)?.Select(a => a.Value).ToList(),

                        EmploymentContactPhone = empContactList.Where(a => a.ProfileId == e.ProfileId && a.ContactType == ContactTypeEnum.OfficePhone && a.IsCurrent)?.Select(a => a.Value).ToList(),
                        EmploymentContactMobile = empContactList.Where(a => a.ProfileId == e.ProfileId && a.ContactType == ContactTypeEnum.OrganizationMobile && a.IsCurrent)?.Select(a => a.Value).ToList(),
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
