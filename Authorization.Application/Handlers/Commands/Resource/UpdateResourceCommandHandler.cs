using Authorization.Application.Commands.Resource;
using Authorization.Application.Interfaces;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Commands.Resource
{
    public class UpdateResourceCommandHandler : IRequestHandler<UpdateResourceCommand, Result<bool>>
    {
        private readonly IResourceInternalService _resourceService;
        private readonly ILogger<UpdateResourceCommandHandler> _logger;

        public UpdateResourceCommandHandler(
            IResourceInternalService resourceService,
            ILogger<UpdateResourceCommandHandler> logger)
        {
            _resourceService = resourceService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateResourceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating resource: {ResourceId}", request.Id);

                await _resourceService.UpdateResourceAsync(request);

                _logger.LogInformation("Resource updated successfully: {ResourceId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update resource: {ResourceId}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
