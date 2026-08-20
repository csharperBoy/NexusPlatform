using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.Location
{
   
    public record DeleteLocationCommand(Guid Id) : IRequest<Result<bool>>;


    public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, Result<bool>>
    {
        private readonly ILocationInternalService _locationService;
        private readonly IPostInternalService _orgChartService;
        private readonly ILogger<DeleteLocationCommandHandler> _logger;

        public DeleteLocationCommandHandler(
            ILocationInternalService locationService,
            ILogger<DeleteLocationCommandHandler> logger,
            IPostInternalService orgChartService)
        {
            _locationService = locationService;
            _logger = logger;
            _orgChartService = orgChartService;
        }

        public async Task<Result<bool>> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Delete Location: {Id}", request.Id);

                await _locationService.DeleteAsync(request.Id);

                await _locationService.SaveAsync();

                _logger.LogInformation("Location Deleted successfully: {LocationId}", request.Id);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to Delete Location: {Id}",
                     request.Id);

                return Result<bool>.Fail(ex.Message);
            }
        }
    }
}
