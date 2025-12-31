using Authorization.Application.Commands.DataScopes;
using Authorization.Application.Interfaces;
using Core.Shared.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Commands.DataScopes
{
    public class AssignDataScopeCommandHandler : IRequestHandler<AssignDataScopeCommand, Result<Guid>>
    {
        //private readonly IDataScopeService _dataScopeService;
        private readonly ILogger<AssignDataScopeCommandHandler> _logger;

        public AssignDataScopeCommandHandler(
            //IDataScopeService dataScopeService,
            ILogger<AssignDataScopeCommandHandler> logger)
        {
            //_dataScopeService = dataScopeService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(AssignDataScopeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "Assigning data scope for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    request.AssigneeType, request.AssigneeId, request.ResourceId);

                //var dataScopeId = await _dataScopeService.AssignDataScopeAsync(request);

                _logger.LogInformation(
                    "Data scope assigned successfully: {DataScopeId}", null);

                return Result<Guid>.Ok(Guid.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to assign data scope for {AssigneeType}:{AssigneeId} to resource {ResourceId}",
                    request.AssigneeType, request.AssigneeId, request.ResourceId);

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }
}
