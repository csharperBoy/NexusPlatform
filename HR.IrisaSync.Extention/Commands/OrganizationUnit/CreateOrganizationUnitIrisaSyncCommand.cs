using Core.Application.Context;
using Core.Application.Provider;
using Core.Domain.Common;
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

namespace HR.IrisaSync.Extention.Commands.OrganizationUnit
{
    public record CreateOrganizationUnitIrisaSyncCommand(
           string Code,
           string Name,
           //Guid? ParentId,
           decimal? IrsiaSyncId,
           decimal? IrsiaSyncParentId
       ) : IRequest<Result<Guid>>;


    public class CreateOrganizationUnitIrisaSyncCommandHandler : IRequestHandler<CreateOrganizationUnitIrisaSyncCommand, Result<Guid>>
    {
        private readonly IOrganizationUnitInternalService _service;
        private readonly IMapService _mapService;
        private readonly ILogger<CreateOrganizationUnitIrisaSyncCommandHandler> _logger;
        private readonly IUserDataContextProvider _userProvider;
        public CreateOrganizationUnitIrisaSyncCommandHandler(
            IOrganizationUnitInternalService service, IMapService mapService,
           IUserDataContextProvider userProvider,
        ILogger<CreateOrganizationUnitIrisaSyncCommandHandler> logger)
        {
            _service = service;
            _mapService = mapService;
            _logger = logger;
            _userProvider = userProvider;
        }

        public async Task<Result<Guid>> Handle(CreateOrganizationUnitIrisaSyncCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating OrganizationUnit: {request.Name}");
                UserDataContext userContext = await _userProvider.GetAsync(new CancellationToken());

                var parentmap = await _mapService.GetOrgUnitByIrisaId(request.IrsiaSyncParentId);
                Guid newId = await _service.CreateAsync(request.Code,
                    request.Name, parentmap?.FkOrganizationUnitId
                  , userContext.UserName
                    );
                await _mapService.SyncOrganizationUnitDoneAsync(newId,request.Name, request.IrsiaSyncId);
                await _mapService.SaveAsync();
                await _service.SaveAsync();
                _logger.LogInformation(
                    $"OrganizationUnit created successfully: {request.Name}");

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
