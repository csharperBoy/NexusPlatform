using Core.Application.Context;
using Core.Application.Provider;
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

namespace HR.IrisaSync.Extention.Commands.JobTitle
{
    public record CreateJobTitleIrisaSyncCommand(
           string Code,
           string Name,
            decimal? IrisaId
       ) : IRequest<Result<Guid>>;


    public class CreateJobTitleIrisaSyncCommandHandler : IRequestHandler<CreateJobTitleIrisaSyncCommand, Result<Guid>>
    {
        private readonly IJobTitleInternalService _service; 
        private readonly IMapService _mapService;

        private readonly ILogger<CreateJobTitleIrisaSyncCommandHandler> _logger;
        private readonly IUserDataContextProvider _userProvider;
        public CreateJobTitleIrisaSyncCommandHandler(
            IJobTitleInternalService service,
            IMapService mapService,
           IUserDataContextProvider userProvider,
        ILogger<CreateJobTitleIrisaSyncCommandHandler> logger)
        {
            _service = service;
            _mapService = mapService;
            _logger = logger;
            _userProvider = userProvider;
        }

        public async Task<Result<Guid>> Handle(CreateJobTitleIrisaSyncCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating JobTitle: {request.Name}");
                UserDataContext userContext = await _userProvider.GetAsync(new CancellationToken());


                Guid newId = await _service.CreateAsync(request.Code,
                    request.Name
                  , userContext.UserName
                    );

                await _mapService.SyncJobTitleDoneAsync(newId,request.Name, request.IrisaId);
                await _mapService.SaveAsync();
                await _service.SaveAsync();
                _logger.LogInformation(
                    $"JobTitle created successfully: {request.Name}");

                return Result<Guid>.Ok(newId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    $"Failed to create Post: {request.Name}");

                return Result<Guid>.Fail(ex.Message);
            }
        }
    }

}
