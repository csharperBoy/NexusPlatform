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
    public class GetDataScopesByUserQueryHandler
        : IRequestHandler<GetDataScopesByUserQuery, Result<IReadOnlyList<DataScopeDto>>>
    {
        private readonly IDataScopeService _dataScopeService;
        private readonly ILogger<GetDataScopesByUserQueryHandler> _logger;

        public GetDataScopesByUserQueryHandler(
            IDataScopeService dataScopeService,
            ILogger<GetDataScopesByUserQueryHandler> logger)
        {
            _dataScopeService = dataScopeService;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<DataScopeDto>>> Handle(
            GetDataScopesByUserQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Getting data scopes for user {UserId}", request.UserId);

                var dataScopes = await _dataScopeService.GetUserDataScopesAsync(request.UserId);
                return Result<IReadOnlyList<DataScopeDto>>.Ok(dataScopes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get data scopes for user {UserId}", request.UserId);
                return Result<IReadOnlyList<DataScopeDto>>.Fail(ex.Message);
            }
        }
    }
}
