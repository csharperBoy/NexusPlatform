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

namespace Contact.Application.Commands.Location
{
    
    public record UpdateLocationContactCommand(
           Guid Id,
     string? OfficePhone,
            string? OrgEmail,
            string? OrgMobile
    


) : IRequest<Result<Guid>>;


    public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationContactCommand, Result<Guid>>
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

        public async Task<Result<Guid>> Handle(UpdateLocationContactCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Update LocationContact: {id}",
                    request.Id);

                Guid LocationId = await _locationService.UpdateLocationAsync(
                    request.Id,
                    null,
                    request.OfficePhone,
                    request.OrgEmail,
                    request.OrgMobile
                    );
                
                await _locationService.SaveAsync();
                _logger.LogInformation(
                    "LocationContact Update successfully: {Id}",
                    LocationId);

                return Result<Guid>.Ok(LocationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to update LocationContact: {id}",
                     request.Id);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}
