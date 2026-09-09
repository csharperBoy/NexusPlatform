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
    public record DeleteJobLevelCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeleteJobLevelCommandHandler : IRequestHandler<DeleteJobLevelCommand, Result<bool>>
    {
        private readonly IJobLevelInternalService _service;
        private readonly ILogger<DeleteJobLevelCommandHandler> _logger;

        public DeleteJobLevelCommandHandler(
            IJobLevelInternalService service,
            ILogger<DeleteJobLevelCommandHandler> logger
            )
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteJobLevelCommand request, CancellationToken cancellationToken)
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
