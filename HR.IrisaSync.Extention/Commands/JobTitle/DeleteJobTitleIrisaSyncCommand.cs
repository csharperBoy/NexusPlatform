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
    public record DeleteJobTitleIrisaSyncCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeleteJobTitleIrisaSyncCommandHandler : IRequestHandler<DeleteJobTitleIrisaSyncCommand, Result<bool>>
    {
        private readonly IJobTitleInternalService _service;
        private readonly ILogger<DeleteJobTitleIrisaSyncCommandHandler> _logger;

        public DeleteJobTitleIrisaSyncCommandHandler(
            IJobTitleInternalService service,
            ILogger<DeleteJobTitleIrisaSyncCommandHandler> logger
            )
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteJobTitleIrisaSyncCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Delete JobTitle: {request.Id}");

                await _service.DeleteAsync(request.Id);

                await _service.SaveAsync();

                _logger.LogInformation($"JobTitle Deleted successfully: {request.Id}");

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to Delete JobTitle: {request.Id}");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }

}
