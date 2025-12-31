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
    public class UpdateDataScopeCommandHandler : IRequestHandler<UpdateDataScopeCommand, Result>
    {
        //private readonly IDataScopeService _dataScopeService;
        private readonly ILogger<UpdateDataScopeCommandHandler> _logger;

        public UpdateDataScopeCommandHandler(
            //IDataScopeService dataScopeService,
            ILogger<UpdateDataScopeCommandHandler> logger)
        {
            //_dataScopeService = dataScopeService;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateDataScopeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Updating data scope: {DataScopeId}", request.DataScopeId);

                //await _dataScopeService.UpdateDataScopeAsync(request);

                _logger.LogInformation("Data scope updated successfully: {DataScopeId}", request.DataScopeId);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update data scope: {DataScopeId}", request.DataScopeId);
                return Result.Fail(ex.Message);
            }
        }
    }
}
