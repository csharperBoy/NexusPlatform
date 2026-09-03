using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.IrisaSync.Extention.Interface;
using HR.IrisaSync.Extention.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Commands
{

    public record SyncWithIrisaCommand : IRequest<Result<Dictionary<string, BatchResult<SyncResult>>>>;


    public class SyncWithIrisaCommandHandler : IRequestHandler<SyncWithIrisaCommand, Result<Dictionary<string, BatchResult<SyncResult>>>>
    {
        private readonly ISyncService _syncService;
        private readonly IMapService _mapService;
        private readonly ILogger<SyncWithIrisaCommandHandler> _logger;

        public SyncWithIrisaCommandHandler(
            ISyncService syncService,
            IMapService mapService,
            ILogger<SyncWithIrisaCommandHandler> logger)
        {
            _syncService = syncService;
            _mapService = mapService;
            _logger = logger;
        }

        public async Task<Result<Dictionary<string, BatchResult<SyncResult>>>> Handle(SyncWithIrisaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sync Hr with irisa system");
                await _mapService.FillJobLevelMap();
                await _mapService.FillJobTitleMap();
                await _mapService.FillOrganizationUnitRootMap();
                await _mapService.FillOrganizationUnitMap();

                BatchResult<SyncResult> orgResult = await _syncService.SyncOrganizationUnitAsync();
                BatchResult<SyncResult> jlResult = await _syncService.SyncJobLevelAsync();
                BatchResult<SyncResult> jtResult = await _syncService.SyncJobTitleAsync();
                BatchResult<SyncResult> postResult = await _syncService.SyncPostAsync();
                BatchResult<SyncResult> empResult = await _syncService.SyncEmploymentsAsync();

                Dictionary<string, BatchResult<SyncResult>> result = new()
                {
                    { "orgUnit", orgResult },
                    { "jobLevel", jlResult },
                    { "jobTitle", jtResult },
                    { "employment", empResult },
                    { "post", postResult }
                };  

                _logger.LogInformation($"Sync successfully: org = Add({orgResult.Data.AddedCount}) | Edit({orgResult.Data.UpdatedCount}), jl = Add({jlResult.Data.AddedCount}) | Edit({jlResult.Data.UpdatedCount}), jt = Add({jtResult.Data.AddedCount}) | Edit({jtResult.Data.UpdatedCount}), emp = Add({empResult.Data.AddedCount}) | Edit({empResult.Data.UpdatedCount}), post = Add({postResult.Data.AddedCount}) | Edit({postResult.Data.UpdatedCount})");

                return Result<Dictionary<string, BatchResult<SyncResult>>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to Sync!!!");

                return Result<Dictionary<string, BatchResult<SyncResult>>>.Fail(ex.Message);
            }
        }
    }
}
