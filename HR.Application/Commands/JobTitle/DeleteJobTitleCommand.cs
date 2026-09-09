using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.JobTitle
{
    public record DeleteJobTitleCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeleteJobTitleCommandHandler : IRequestHandler<DeleteJobTitleCommand, Result<bool>>
    {
        private readonly IJobTitleInternalService _service;
        private readonly ILogger<DeleteJobTitleCommandHandler> _logger;

        public DeleteJobTitleCommandHandler(
            IJobTitleInternalService service,
            ILogger<DeleteJobTitleCommandHandler> logger
            )
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteJobTitleCommand request, CancellationToken cancellationToken)
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
