using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Commands.OrganizationUnit
{
   
    public record DeleteOrganizationUnitIrisaSyncCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeleteOrganizationUnitIrisaSyncCommandHandler : IRequestHandler<DeleteOrganizationUnitIrisaSyncCommand, Result<bool>>
    {
        private readonly IOrganizationUnitInternalService _service;
        private readonly ILogger<DeleteOrganizationUnitIrisaSyncCommandHandler> _logger;

        public DeleteOrganizationUnitIrisaSyncCommandHandler(
            IOrganizationUnitInternalService service,
            ILogger<DeleteOrganizationUnitIrisaSyncCommandHandler> logger
            )
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteOrganizationUnitIrisaSyncCommand request, CancellationToken cancellationToken)
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
