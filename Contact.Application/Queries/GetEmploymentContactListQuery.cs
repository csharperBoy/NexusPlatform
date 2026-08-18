using Contact.Application.DTOs;
using Core.Application.Abstractions.Contact;
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
        private readonly IHrContactPublicService _hrContactService;
        private readonly IPeopleContactPublicService _peopleContactService;
        public GetEmploymentContactListQueryHandler(
            IEmploymentInternalService employmentInternalService,
            IHrContactPublicService hrContactService,
        IPeopleContactPublicService peopleContactService,


        ILogger<GetEmploymentContactListQueryHandler> logger)
        {
            _employmentInternalService = employmentInternalService;
            _hrContactService = hrContactService;
            _peopleContactService = peopleContactService;
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
                var emptIds = employments.Select(p => p.Id).ToList();

                var hrContactList = await _hrContactService.GetEmploymentContactsByEmploymentIdsAsync(emptIds);
                var peopleContactList = await _peopleContactService.GetPartyContactsByPartyIdsAsync(emptIds);

                IReadOnlyList<EmploymentContactDto> result = employments
                    .Where(e => string.IsNullOrEmpty(request.employmentCode) || e.EmploymentCode == request.employmentCode)
                    .Select(e => new EmploymentContactDto
                    {
                        Id = e.Id,
                        NationalCode = e.NationalCode,
                        FirstName = e.FirstName,
                        LastName = e.LastName,
                        EmploymentCode = e.EmploymentCode,
                        PartyMobile = peopleContactList.Where(a => a.EntityId == e.PartyId && a.ContactType == PartyContactType.Mobile && a.IsCurrent)?.Select(a=>a.Value).ToList(),
                        PartyAddress = peopleContactList.Where(a => a.EntityId == e.PartyId && a.ContactType == PartyContactType.Address && a.IsCurrent)?.Select(a => a.Value).ToList(),
                        PartyPhone = peopleContactList.Where(a => a.EntityId == e.PartyId && a.ContactType == PartyContactType.Phone && a.IsCurrent)?.Select(a => a.Value).ToList(),
                        PartyEmail = peopleContactList.Where(a => a.EntityId == e.PartyId && a.ContactType == PartyContactType.Email && a.IsCurrent)?.Select(a => a.Value).ToList(),

                        EmploymentContactPhone = hrContactList.Where(a => a.EntityId == e.Id && a.ContactType == HrContactType.OfficePhone && a.IsCurrent)?.Select(a => a.Value).ToList(),
                        EmploymentContactMobile = hrContactList.Where(a => a.EntityId == e.Id && a.ContactType == HrContactType.OrgMobile && a.IsCurrent)?.Select(a => a.Value).ToList(),
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
