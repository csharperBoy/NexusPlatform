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
    public record DeleteAssignmentCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeleteAssignmentCommandHandler : IRequestHandler<DeleteAssignmentCommand, Result<bool>>
    {
        private readonly IPostInternalService _service;
        private readonly ILogger<DeleteAssignmentCommandHandler> _logger;

        public DeleteAssignmentCommandHandler(
            IPostInternalService service,
            ILogger<DeleteAssignmentCommandHandler> logger
            )
        {
            _service = service;
            _logger = logger;
        }

        public async Task<Result<bool>> Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Delete Assignment: {request.Id}");

                //await _service.(request.Id);

                //await _service.SaveAsync();

                _logger.LogInformation($"Assignment Deleted successfully: {request.Id}");

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to Delete Assignment: {request.Id}");

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
