using Contact.Application.DTOs;
using Core.Application.Abstractions.Contact;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using Core.Shared.Results;
using HR.Application.DTOs;
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
    public record GetLocationContactListQuery(string? locationCode = null)
    : IRequest<Result<IReadOnlyList<LocationContactDto>>>;

    public class GetLocationContactListQueryHandler
        : IRequestHandler<GetLocationContactListQuery, Result<IReadOnlyList<LocationContactDto>>>
    {
        private readonly ILocationInternalService _locationInternalService;
        private readonly IHrContactPublicService _hrContactService;
        private readonly ILogger<GetLocationContactListQueryHandler> _logger;

        public GetLocationContactListQueryHandler(
            ILocationInternalService locationInternalService, IHrContactPublicService hrContactService,
        ILogger<GetLocationContactListQueryHandler> logger)
        {
            _locationInternalService = locationInternalService;
            _hrContactService = hrContactService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<LocationContactDto>>> Handle(
            GetLocationContactListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting LocationContact List:");

                var locations = await _locationInternalService.GetLocationListAsync();
                var locIds = locations.Select(p => p.Id).ToList();

                var hrContactList = await _hrContactService.GetLocationContactsByLocationIdsAsync(locIds);

                IReadOnlyList<LocationContactDto> result = locations
                    .Select(e => new LocationContactDto
                    {
                        Id = e.Id,
                       Title = e.Title,
                        orgPhone = hrContactList.Where(a => a.EntityId == e.Id && a.ContactType == HrContactType.OfficePhone)?.Select(a => a.Value).ToList(),
                        orgMobile = hrContactList.Where(a => a.EntityId == e.Id && a.ContactType == HrContactType.OrgMobile)?.Select(a => a.Value).ToList(),

                    })
                    .ToList();
                return Result<IReadOnlyList<LocationContactDto>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get LocationContact List");
                return Result<IReadOnlyList<LocationContactDto>>.Fail(ex.Message);
            }
        }
    }
}
