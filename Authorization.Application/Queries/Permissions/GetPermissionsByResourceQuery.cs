using Authorization.Application.DTOs.Permissions;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.Permissions
{
    /*
   📌 GetPermissionsByResourceQuery
   --------------------------------
   Query برای دریافت Permission های یک Resource.
  */
    public record GetPermissionsByResourceQuery(Guid ResourceId)
        : IRequest<Result<IReadOnlyList<PermissionDto>>>;
}
