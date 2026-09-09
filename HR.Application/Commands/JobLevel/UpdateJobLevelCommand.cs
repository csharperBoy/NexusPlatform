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

namespace HR.Application.Commands.JobLevel
{
   

    public record UpdateJobLevelCommand(
           Guid Id,
   Optional<string?> Code,
   Optional<string?> Title




) : IRequest<Result<bool>>;


    public class UpdateJobLevelCommandHandler : IRequestHandler<UpdateJobLevelCommand, Result<bool>>
    {
        private readonly IJobLevelInternalService _service;
        private readonly ILogger<UpdateJobLevelCommandHandler> _logger;

        public UpdateJobLevelCommandHandler(
            IJobLevelInternalService service,
            ILogger<UpdateJobLevelCommandHandler> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateJobLevelCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating JobLevel: {request.Title}");

                bool hasChange = await _service.UpdateAsync(
                      request.Id,
                      request.Title,
                      request.Code
                      );
                //Guid JobLevelId = request.Id;
                await _service.SaveAsync();
                _logger.LogInformation(
                    $"JobLevel updated successfully: {request.Title}");

                return Result<bool>.Ok(hasChange);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to update JobLevel: {request.Title}");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
