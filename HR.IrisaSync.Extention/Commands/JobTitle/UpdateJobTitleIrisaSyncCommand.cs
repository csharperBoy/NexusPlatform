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

namespace HR.IrisaSync.Extention.Commands.JobTitle
{
    public record UpdateJobTitleIrisaSyncCommand(
           Guid Id,
   Optional<string?> Code,
   Optional<string?> Name,
   Optional<bool> IsActive
) : IRequest<Result<bool>>;


    public class UpdateJobTitleIrisaSyncCommandHandler : IRequestHandler<UpdateJobTitleIrisaSyncCommand, Result<bool>>
    {
        private readonly IJobTitleInternalService _service;
        private readonly ILogger<UpdateJobTitleIrisaSyncCommandHandler> _logger;

        public UpdateJobTitleIrisaSyncCommandHandler(
            IJobTitleInternalService service,
            ILogger<UpdateJobTitleIrisaSyncCommandHandler> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(UpdateJobTitleIrisaSyncCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating JobTitle: {request.Name}");

                bool hasChange = await _service.UpdateAsync(
                      request.Id,
                      request.Name,
                      request.Code,
                      request.IsActive
                      );
                //Guid JobTitleId = request.Id;
                await _service.SaveAsync();
                _logger.LogInformation(
                    $"JobTitle updated successfully: {request.Name}");

                return Result<bool>.Ok(hasChange);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to update JobTitle: {request.Name}");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
