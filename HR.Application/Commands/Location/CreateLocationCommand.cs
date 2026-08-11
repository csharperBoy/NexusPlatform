using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Provider;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.Location
{
    public record CreateLocationCommand(
   string Title,
    string? OfficePhone,
           string? OrgEmail,
           string? OrgMobile
    

) : IRequest<Result<Guid>>;


   

    public class CreateLocationCommandHandler : IRequestHandler<CreateLocationCommand, Result<Guid>>
    {
        private readonly ILocationInternalService _locationService;
        private readonly ILogger<CreateLocationCommandHandler> _logger;

        public CreateLocationCommandHandler(
            ILocationInternalService locationService,
            ILogger<CreateLocationCommandHandler> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating resource: {Title}",
                    request.Title);

                Guid locationId = await _locationService.CreateLocationAsync(
                       request.Title, request.OfficePhone, request.OrgEmail, request.OrgMobile
                    );
              

                await _locationService.SaveAsync();
                _logger.LogInformation(
                    "Location created successfully: {locationId} ({Title})",
                    locationId, request.Title);

                return Result<Guid>.Ok(locationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create Location: {Title}",
                     request.Title);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}
