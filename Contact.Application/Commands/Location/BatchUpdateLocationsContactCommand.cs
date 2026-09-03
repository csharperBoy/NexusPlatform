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

namespace Contact.Application.Commands.Location
{
    public record BatchUpdateLocationsContactCommand(List<UpdateLocationContactCommand> Locations) : IRequest<Result<List<Guid>>>;


    public class BatchUpdateLocationsCommandHandler : IRequestHandler<BatchUpdateLocationsContactCommand, Result<List<Guid>>>
    {
        private readonly ILocationInternalService _locationService;
        private readonly ILogger<BatchUpdateLocationsCommandHandler> _logger;

        public BatchUpdateLocationsCommandHandler( ILogger<BatchUpdateLocationsCommandHandler> logger, ILocationInternalService locationService)
        {
            _logger = logger;
            _locationService = locationService;
        }

        public async Task<Result<List<Guid>>> Handle(BatchUpdateLocationsContactCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var results = new List<Guid>();
                foreach (var command in request.Locations)
                {
                    // ۱. به‌روزرسانی اطلاعات پایه پست
                 bool hasChange =    await _locationService.UpdateLocationAsync(
                   command.Id,

                    Optional<string?>.Undefined,
                   command.OfficePhone,
                   command.OrgEmail,
                   command.OrgMobile
                   );
                    Guid LocationId = command.Id;

                    results.Add(LocationId);
                }

                await _locationService.SaveAsync();

                _logger.LogInformation("Batch update of {count} LocationContact completed successfully.", results.Count);
                return Result<List<Guid>>.Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to batch update LocationContact.");
                return Result<List<Guid>>.Fail(ex.Message);
            }
        }
    }
}
