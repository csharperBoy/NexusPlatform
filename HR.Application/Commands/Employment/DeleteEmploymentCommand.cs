using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.Employment
{
    public record DeleteEmploymentCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeleteEmploymentCommandHandler : IRequestHandler<DeleteEmploymentCommand, Result<bool>>
    {
        private readonly IEmploymentInternalService _employmentService;
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<DeleteEmploymentCommandHandler> _logger;

        public DeleteEmploymentCommandHandler(
            IEmploymentInternalService employmentService,
            ILogger<DeleteEmploymentCommandHandler> logger,
            IPostInternalService orgChartService)
        {
            _employmentService = employmentService;
            _logger = logger;
            _orgChartService = orgChartService;
        }

        public async Task<Result<bool>> Handle(DeleteEmploymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Delete Employment: {Id}", request.Id);

                await _employmentService.DeleteAsync(request.Id);

                await _employmentService.SaveAsync();

                _logger.LogInformation("Employment Deleted successfully: {EmploymentId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to Delete Employment: {Id}",
                     request.Id);

                return Result<bool>.Fail(ex.Message);
            }
        }
    }

}
