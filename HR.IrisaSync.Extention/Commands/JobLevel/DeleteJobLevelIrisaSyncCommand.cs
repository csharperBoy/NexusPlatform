using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.IrisaSync.Extention.Interface;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Commands.JobLevel
{
    public record DeleteJobLevelIrisaSyncCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeleteJobLevelIrisaSyncCommandHandler : IRequestHandler<DeleteJobLevelIrisaSyncCommand, Result<bool>>
    {
        private readonly IJobLevelInternalService _service;
        private readonly ILogger<DeleteJobLevelIrisaSyncCommandHandler> _logger;

        public DeleteJobLevelIrisaSyncCommandHandler(
            IJobLevelInternalService service, 
            ILogger<DeleteJobLevelIrisaSyncCommandHandler> logger
            )
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteJobLevelIrisaSyncCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Delete JobLevel: {request.Id}");

                await _service.DeleteAsync(request.Id);

                await _service.SaveAsync();

                _logger.LogInformation($"JobLevel Deleted successfully: {request.Id}");

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to Delete JobLevel: {request.Id}");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
