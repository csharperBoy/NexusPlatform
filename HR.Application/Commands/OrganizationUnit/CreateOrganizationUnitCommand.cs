using Core.Application.Context;
using Core.Application.Provider;
using Core.Shared.Results;
using HR.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.Commands.OrganizationUnit
{
    public record CreateOrganizationUnitCommand(
           string Code,
           string Name,
           Guid? ParentId
       ) : IRequest<Result<Guid>>;


    public class CreateOrganizationUnitCommandHandler : IRequestHandler<CreateOrganizationUnitCommand, Result<Guid>>
    {
        private readonly IOrganizationUnitInternalService _service;
        private readonly ILogger<CreateOrganizationUnitCommandHandler> _logger;
        private readonly IUserDataContextProvider _userProvider;
        public CreateOrganizationUnitCommandHandler(
            IOrganizationUnitInternalService service,
           IUserDataContextProvider userProvider,
        ILogger<CreateOrganizationUnitCommandHandler> logger)
        {
            _service = service;
            _logger = logger;
            _userProvider = userProvider;
        }

        public async Task<Result<Guid>> Handle(CreateOrganizationUnitCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    $"Creating OrganizationUnit: {request.Name}");
                UserDataContext userContext = await _userProvider.GetAsync(new CancellationToken());


                Guid newId = await _service.CreateAsync(request.Code,
                    request.Name,request.ParentId
                  , userContext.UserName
                    );

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
