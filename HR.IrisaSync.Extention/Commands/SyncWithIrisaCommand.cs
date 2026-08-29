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

    public record SyncWithIrisaCommand : IRequest<Result<Dictionary<string, SyncResult>>>;


    public class SyncWithIrisaCommandHandler : IRequestHandler<SyncWithIrisaCommand, Result<Dictionary<string, SyncResult>>>
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

        public async Task<Result<Dictionary<string, SyncResult>>> Handle(SyncWithIrisaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Sync Hr with irisa system");
                await _mapService.FillJobLevelMap();
                await _mapService.FillJobTitleMap();
                await _mapService.FillOrganizationUnitMap();

                SyncResult orgResult = await _syncService.SyncOrganizationUnitAsync();
                SyncResult jlResult = await _syncService.SyncJobLevelAsync();
                SyncResult jtResult = await _syncService.SyncJobTitleAsync();
                SyncResult empResult = await _syncService.SyncEmploymentsAsync();
                SyncResult postResult = await _syncService.SyncPostAsync();

                Dictionary<string, SyncResult> result = new()
                {
                    { "orgUnit", orgResult },
                    { "jobLevel", jlResult },
                    { "jobTitle", jtResult },
                    { "employment", empResult },
                    { "post", postResult }
                };  

                _logger.LogInformation($"Sync successfully: org = Add({orgResult.AddedCount}) | Edit({orgResult.UpdatedCount}), jl = Add({jlResult.AddedCount}) | Edit({jlResult.UpdatedCount}), jt = Add({jtResult.AddedCount}) | Edit({jtResult.UpdatedCount}), emp = Add({empResult.AddedCount}) | Edit({empResult.UpdatedCount}), post = Add({postResult.AddedCount}) | Edit({postResult.UpdatedCount})");

                return Result<Dictionary<string, SyncResult>>.Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to Sync!!!");

                return Result<Dictionary<string, SyncResult>>.Fail(ex.Message);
            }
        }
    }
}
