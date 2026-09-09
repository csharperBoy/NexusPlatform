using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.OrganizationUnit
{
   
    public record DeleteOrganizationUnitCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeleteOrganizationUnitCommandHandler : IRequestHandler<DeleteOrganizationUnitCommand, Result<bool>>
    {
        private readonly IOrganizationUnitInternalService _service;
        private readonly ILogger<DeleteOrganizationUnitCommandHandler> _logger;

        public DeleteOrganizationUnitCommandHandler(
            IOrganizationUnitInternalService service,
            ILogger<DeleteOrganizationUnitCommandHandler> logger
            )
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteOrganizationUnitCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Delete OrganizationUnit: {request.Id}");

                await _service.DeleteAsync(request.Id);

                await _service.SaveAsync();

                _logger.LogInformation($"OrganizationUnit Deleted successfully: {request.Id}");

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to Delete OrganizationUnit: {request.Id}");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
