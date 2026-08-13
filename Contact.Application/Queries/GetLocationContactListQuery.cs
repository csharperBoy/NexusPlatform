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

namespace Contact.Application.Queries
{
    public record GetLocationContactListQuery(string? locationCode = null)
    : IRequest<Result<IReadOnlyList<LocationInfoDto>>>;

    public class GetLocationContactListQueryHandler
        : IRequestHandler<GetLocationContactListQuery, Result<IReadOnlyList<LocationInfoDto>>>
    {
        private readonly ILocationInternalService _locationInternalService;
        private readonly ILogger<GetLocationContactListQueryHandler> _logger;

        public GetLocationContactListQueryHandler(
            ILocationInternalService locationInternalService,
        ILogger<GetLocationContactListQueryHandler> logger)
        {
            _locationInternalService = locationInternalService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<LocationInfoDto>>> Handle(
            GetLocationContactListQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting LocationContact List:");

                var locations = await _locationInternalService.GetLocationListAsync();
                return Result<IReadOnlyList<LocationInfoDto>>.Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get LocationContact List");
                return Result<IReadOnlyList<LocationInfoDto>>.Fail(ex.Message);
            }
        }
    }
}
