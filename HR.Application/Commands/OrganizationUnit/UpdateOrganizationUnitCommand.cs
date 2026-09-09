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

namespace HR.Application.Commands.OrganizationUnit
{
    public record UpdateOrganizationUnitCommand(
          Guid Id,
  Optional<string?> Code,
  Optional<string?> Name,
  Optional<Guid?> ParentId
) : IRequest<Result<bool>>;


    public class UpdateOrganizationUnitCommandHandler : IRequestHandler<UpdateOrganizationUnitCommand, Result<bool>>
    {
        private readonly IOrganizationUnitInternalService _service;
        private readonly ILogger<UpdateOrganizationUnitCommandHandler> _logger;

        public UpdateOrganizationUnitCommandHandler(
            IOrganizationUnitInternalService service,
            ILogger<UpdateOrganizationUnitCommandHandler> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateOrganizationUnitCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating OrganizationUnit: {request.Name}");

                bool hasChange = await _service.UpdateAsync(
                      request.Id,
                      request.Name,
                      request.Code,request.ParentId
                      );
                //Guid OrganizationUnitId = request.Id;
                await _service.SaveAsync();
                _logger.LogInformation(
                    $"OrganizationUnit update successfully: {request.Name}");

                return Result<bool>.Ok(hasChange);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to update OrganizationUnit: {request.Name}");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
