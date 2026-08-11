using Core.Shared.Results;
using HR.Application.DTOs;
using HR.Application.Interfaces;
using HR.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Queries.Location
{
    public record GetLocationListQuery(string? locationCode = null)
    : IRequest<Result<IReadOnlyList<LocationInfoDto>>>;

    public class GetLocationListQueryHandler
        : IRequestHandler<GetLocationListQuery, Result<IReadOnlyList<LocationInfoDto>>>
    {
        private readonly ILocationInternalService _locationInternalService;
        private readonly ILogger<GetLocationListQueryHandler> _logger;

        public GetLocationListQueryHandler(
            ILocationInternalService locationInternalService,
        ILogger<GetLocationListQueryHandler> logger)
        {
            _locationInternalService = locationInternalService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<LocationInfoDto>>> Handle(
            GetLocationListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting Location List:");

                var locations = await _locationInternalService.GetLocationListAsync();
                return Result<IReadOnlyList<LocationInfoDto>>.Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get Location List");
                return Result<IReadOnlyList<LocationInfoDto>>.Fail(ex.Message);
            }
        }
    }
}
