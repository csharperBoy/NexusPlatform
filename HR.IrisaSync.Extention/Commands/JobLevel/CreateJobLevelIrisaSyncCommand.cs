using Core.Application.Abstractions.People;
using Core.Application.Context;
using Core.Application.Provider;
using Core.Domain.ValueObjects;
using Core.Shared.Enums.HR;
using Core.Shared.Results;
using HR.Application.Interfaces;
using HR.IrisaSync.Extention.Interface;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.IrisaSync.Extention.Commands.JobLevel
{
    public record CreateJobLevelIrisaSyncCommand(
            string Code,
            string Title,
            decimal? IrisaId
        ) : IRequest<Result<Guid>>;


    public class CreateJobLevelIrisaSyncCommandHandler : IRequestHandler<CreateJobLevelIrisaSyncCommand, Result<Guid>>
    {
        private readonly IJobLevelInternalService _service;
        private readonly IMapService _mapService;
        private readonly ILogger<CreateJobLevelIrisaSyncCommandHandler> _logger;
        private readonly IUserDataContextProvider _userProvider;
        public CreateJobLevelIrisaSyncCommandHandler(
            IJobLevelInternalService service, 
            IMapService mapService,
        IUserDataContextProvider userProvider,
        ILogger<CreateJobLevelIrisaSyncCommandHandler> logger)
        {
            _service = service;
            _mapService = mapService;
            _logger = logger;
            _userProvider = userProvider;
        }

        public async Task<Result<Guid>> Handle(CreateJobLevelIrisaSyncCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating JobLevel: {request.Title}");
                UserDataContext userContext = await _userProvider.GetAsync(new CancellationToken());


                Guid newId = await _service.CreateAsync(request.Code,
                    request.Title
                  , userContext.UserName
                    );
                await _mapService.SyncJobLevelDoneAsync(newId,request.Title, request.IrisaId);
                await _mapService.SaveAsync();
                await _service.SaveAsync();
                _logger.LogInformation(
                    $"JobLevel created successfully: {request.Title}");

                return Result<Guid>.Ok(newId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to create Post: {request.Title}");

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
