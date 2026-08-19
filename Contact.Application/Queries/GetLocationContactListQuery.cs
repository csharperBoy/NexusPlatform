using Contact.Application.DTOs;
using Core.Application.Abstractions.Contact;
using Core.Shared.Enums.Contact;
using Core.Shared.Enums.HR;
 
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
        private readonly IContactPublicService _contactService;
        private readonly ILogger<GetLocationContactListQueryHandler> _logger;

        public GetLocationContactListQueryHandler(
            ILocationInternalService locationInternalService, IContactPublicService contactService,
        ILogger<GetLocationContactListQueryHandler> logger)
        {
            _locationInternalService = locationInternalService;
            _contactService = contactService;
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
                var locProfIds = locations.Select(p => p.ProfileId).ToList();

                var contactList = await _contactService.GetContactsByProfilesIdsAsync(locProfIds);

                IReadOnlyList<LocationContactDto> result = locations
                    .Select(e => new LocationContactDto
                    {
                        Id = e.Id,
                       Title = e.Title,
                        orgPhone = contactList.Where(a => a.ProfileId == e.ProfileId && a.ContactType == ContactTypeEnum.OfficePhone && a.IsCurrent)?.Select(a => a.Value).ToList(),
                        orgMobile = contactList.Where(a => a.ProfileId == e.ProfileId && a.ContactType == ContactTypeEnum.OrganizationMobile && a.IsCurrent)?.Select(a => a.Value).ToList(),

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
