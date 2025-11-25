using Authorization.Application.DTOs.Permissions;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.Users
{
    /*
     📌 GetUserEffectiveAccessQuery
     ------------------------------
     Query برای دریافت دسترسی نهایی (Permissions + DataScopes) یک کاربر.
    */
    public record GetUserEffectiveAccessQuery(Guid UserId)
        : IRequest<Result<UserAccessDto>>;
}
