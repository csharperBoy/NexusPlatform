using Core.Domain.Common;
using Core.Shared.Results;
using HR.Application.Interfaces;
 
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
   Optional<string?> Title
     
    


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

              bool hasChange =  await _locationService.UpdateLocationAsync(
                    request.Id,
                    request.Title,
                    Optional<List<string>?>.Undefined,
                    Optional<List<string>?>.Undefined,
                    Optional<List<string>?>.Undefined
                    );
                Guid LocationId = request.Id;
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
