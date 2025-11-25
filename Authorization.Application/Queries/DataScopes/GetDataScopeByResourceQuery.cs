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
       📌 GetDataScopeByResourceQuery
       ------------------------------
       Query برای دریافت DataScope های مرتبط با یک Resource.
      */
    public record GetDataScopeByResourceQuery(Guid ResourceId)
        : IRequest<Result<IReadOnlyList<DataScopeDto>>>;
}
