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
    public class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand, Result<bool>>
    {
        private readonly IResourceService _resourceService;
        private readonly ILogger<DeleteResourceCommandHandler> _logger;

        public DeleteResourceCommandHandler(
            IResourceService resourceService,
            ILogger<DeleteResourceCommandHandler> logger)
        {
            _resourceService = resourceService;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Deleting resource: {ResourceId}", request.Id);

                await _resourceService.DeleteResourceAsync(request.Id);

                _logger.LogInformation("Resource deleted successfully: {ResourceId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete resource: {ResourceId}", request.Id);
                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
