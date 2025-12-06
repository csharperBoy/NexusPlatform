using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Core.Application.Abstractions.Security;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Commands
{
    public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, Result<Guid>>
    {
        private readonly IResourceService _resourceService;
        private readonly ILogger<CreateResourceCommandHandler> _logger;

        public CreateResourceCommandHandler(
            IResourceService resourceService,
            ILogger<CreateResourceCommandHandler> logger)
        {
            _resourceService = resourceService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Creating resource: {ResourceKey} ({ResourceName})",
                    request.Key, request.Name);

                var resourceId = await _resourceService.CreateResourceAsync(request);

                _logger.LogInformation(
                    "Resource created successfully: {ResourceId} ({ResourceKey})",
                    resourceId, request.Key);

                return Result<Guid>.Ok(resourceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create resource: {ResourceKey} ({ResourceName})",
                    request.Key, request.Name);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}
