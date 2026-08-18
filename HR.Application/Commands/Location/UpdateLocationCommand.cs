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
    
    public record UpdateLocationCommand(
           Guid Id,
   string Title,
     List<string>? OfficePhone,
            List<string>? OrgEmail,
            List<string>? OrgMobile
    


) : IRequest<Result<Guid>>;


    public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommand, Result<Guid>>
    {
        private readonly ILocationInternalService _locationService;
        private readonly ILogger<UpdateLocationCommandHandler> _logger;

        public UpdateLocationCommandHandler(
            ILocationInternalService locationService,
            ILogger<UpdateLocationCommandHandler> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating location: {Title}",
                    request.Title);

                Guid LocationId = await _locationService.UpdateLocationAsync(
                    request.Id,
                    request.Title,
                    request.OfficePhone,
                    request.OrgEmail,
                    request.OrgMobile
                    );
                
                await _locationService.SaveAsync();
                _logger.LogInformation(
                    "Location created successfully: {Id} ({Title})",
                    LocationId, request.Title);

                return Result<Guid>.Ok(LocationId);
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
