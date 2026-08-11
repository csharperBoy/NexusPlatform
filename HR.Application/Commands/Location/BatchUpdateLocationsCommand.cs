using Core.Shared.Results;
using HR.Application.Commands.Location;
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
    public record BatchUpdateLocationsCommand(List<UpdateLocationCommand> Locations) : IRequest<Result<List<Guid>>>;


    public class BatchUpdateLocationsCommandHandler : IRequestHandler<BatchUpdateLocationsCommand, Result<List<Guid>>>
    {
        private readonly ILocationInternalService _locationService;
        private readonly ILogger<BatchUpdateLocationsCommandHandler> _logger;

        public BatchUpdateLocationsCommandHandler( ILogger<BatchUpdateLocationsCommandHandler> logger, ILocationInternalService locationService)
        {
            _logger = logger;
            _locationService = locationService;
        }

        public async Task<Result<List<Guid>>> Handle(BatchUpdateLocationsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var results = new List<Guid>();
                foreach (var command in request.Locations)
                {
                    // ۱. به‌روزرسانی اطلاعات پایه پست
                    Guid LocationId = await _locationService.UpdateLocationAsync(
                   command.Id,
                   command.Title,
                   command.OfficePhone,
                   command.OrgEmail,
                   command.OrgMobile
                   );
                    

                    results.Add(LocationId);
                }

                await _locationService.SaveAsync();

                _logger.LogInformation("Batch update of {count} Location completed successfully.", results.Count);
                return Result<List<Guid>>.Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to batch update posts.");
                return Result<List<Guid>>.Fail(ex.Message);
            }
        }
    }
}
