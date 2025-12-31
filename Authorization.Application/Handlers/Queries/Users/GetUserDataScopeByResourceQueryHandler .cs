using Authorization.Application.DTOs.DataScopes;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries.Users;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries.Users
{
    public class GetUserDataScopeByResourceQueryHandler
        : IRequestHandler<GetUserDataScopeByResourceQuery, Result<DataScopeDto>>
    {
        private readonly IDataScopeEvaluator _dataScopeEvaluator;
        private readonly ILogger<GetUserDataScopeByResourceQueryHandler> _logger;

        public GetUserDataScopeByResourceQueryHandler(
            IDataScopeEvaluator dataScopeEvaluator,
            ILogger<GetUserDataScopeByResourceQueryHandler> logger)
        {
            _dataScopeEvaluator = dataScopeEvaluator;
            _logger = logger;
        }

        public async Task<Result<DataScopeDto>> Handle(
            GetUserDataScopeByResourceQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug(
                    "Getting user data scope for user {UserId} on resource {ResourceKey}",
                    request.UserId, request.ResourceKey);

                var dataScope = await _dataScopeEvaluator.EvaluateDataScopeAsync(
                    request.ResourceKey);

                return Result<DataScopeDto>.Ok(dataScope);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to get user data scope for user {UserId} on resource {ResourceKey}",
                    request.UserId, request.ResourceKey);

                return Result<DataScopeDto>.Fail(ex.Message);
            }
        }
    }
}
