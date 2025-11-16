using Authorization.Application.DTOs;
using Authorization.Application.Interfaces;
using Authorization.Application.Queries;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Handlers.Queries
{
    public class GetAllPermissionsQueryHandler
        : IRequestHandler<GetAllPermissionsQuery, Result<List<PermissionDto>>>
    {
        private readonly IPermissionService _service;

        public GetAllPermissionsQueryHandler(IPermissionService service)
        {
            _service = service;
        }

        public async Task<Result<List<PermissionDto>>> Handle(
            GetAllPermissionsQuery request, CancellationToken ct)
        {
            var list = await _service.GetAllAsync(ct);
            return Result<List<PermissionDto>>.Success(list);
        }
    }

}
