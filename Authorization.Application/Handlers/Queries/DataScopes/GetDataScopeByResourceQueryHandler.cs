using Authorization.Application.DTOs.DataScopes;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.DataScopes;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.DataScopes
{
    public class GetDataScopeByResourceQueryHandler
        : IRequestHandler<GetDataScopeByResourceQuery, Result<IReadOnlyList<DataScopeDto>>>
    {
        //private readonly IDataScopeService _dataScopeService;
        private readonly ILogger<GetDataScopeByResourceQueryHandler> _logger;

        public GetDataScopeByResourceQueryHandler(
            //IDataScopeService dataScopeService,
            ILogger<GetDataScopeByResourceQueryHandler> logger)
        {
            //_dataScopeService = dataScopeService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<DataScopeDto>>> Handle(
            GetDataScopeByResourceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting data scopes for resource {ResourceId}", request.ResourceId);

                //var dataScopes = await _dataScopeService.GetUserDataScopesAsync(request.ResourceId);
                return Result<IReadOnlyList<DataScopeDto>>.Ok(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get data scopes for resource {ResourceId}", request.ResourceId);
                return Result<IReadOnlyList<DataScopeDto>>.Fail(ex.Message);
            }
        }
    }
}
