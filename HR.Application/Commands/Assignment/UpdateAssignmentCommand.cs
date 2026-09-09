using Core.Domain.Common;
using Core.Shared.Enums.HR;
using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.Assignment
{
    public record UpdateAssignmentCommand(
            List<Guid?> postIds,
            Guid employmentId,
             PostAssignmentType? assigneType = null,
             DateTime? effectiveFrom = null,
             DateTime? effectiveTo = null




) : IRequest<Result<bool>>;


    public class UpdateAssignmentCommandHandler : IRequestHandler<UpdateAssignmentCommand, Result<bool>>
    {
        private readonly IPostInternalService _service;
        private readonly ILogger<UpdateAssignmentCommandHandler> _logger;

        public UpdateAssignmentCommandHandler(
            IPostInternalService service,
            ILogger<UpdateAssignmentCommandHandler> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateAssignmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating Assignment");

                bool hasChange = await _service.AssignToEmploymentAsync(request.postIds,
                    request.employmentId, request.assigneType, request.effectiveFrom, request.effectiveTo
                    );

                await _service.SaveAsync();
                _logger.LogInformation(
                    $"Assignment updated successfully)");

                return Result<bool>.Ok(hasChange);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to update Assignment");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
