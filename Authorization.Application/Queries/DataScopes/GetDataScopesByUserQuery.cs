using Authorization.Application.DTOs.DataScopes;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.DataScopes
{
    /*
     📌 GetDataScopesByUserQuery
     ---------------------------
     Query برای دریافت DataScope های مرتبط با یک کاربر (AssigneeType.Person).
    */
    public record GetDataScopesByUserQuery(Guid UserId)
        : IRequest<Result<IReadOnlyList<DataScopeDto>>>;
}
