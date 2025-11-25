using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Core.Application.Abstractions.Security;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Commands
{
    public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, Result<ResourceDto>>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IPermissionChecker _permissionChecker;

        public CreateResourceCommandHandler(IAuthorizationService authorizationService, IPermissionChecker permissionChecker)
        {
            _authorizationService = authorizationService;
            _permissionChecker = permissionChecker;
        }

        public async Task<Result<ResourceDto>> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
        {
            if (!await _permissionChecker.HasPermissionAsync("Authorization.Resource.Create"))
                return Result<ResourceDto>.Fail("اجازه دسترسی وجود ندارد.");

            var created = await _authorizationService.CreateResourceAsync(request, cancellationToken);
            if (!created.Succeeded)
                return Result<ResourceDto>.Fail(created.Error);

            return Result<ResourceDto>.Ok(created.Data);
        }
    }
}
