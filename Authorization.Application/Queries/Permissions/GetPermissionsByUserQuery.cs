using Authorization.Application.DTOs.Permissions;
using Authorization.Application.DTOs.Users;
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
     📌 GetPermissionsByUserQuery
     ----------------------------
     Query برای دریافت خلاصه دسترسی های کاربر.
    */
    public record GetPermissionsByUserQuery(Guid UserId)
        : IRequest<Result<IReadOnlyList<PermissionDto>>>;
}
