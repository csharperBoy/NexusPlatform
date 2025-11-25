using Authorization.Application.DTOs.Resource;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.Resource
{
    /*
     📌 GetResourceTreeQuery
     -----------------------
     Query برای دریافت درخت Resource. RootId اختیاری است.
    */
    public record GetResourceTreeQuery(Guid? RootId = null)
        : IRequest<Result<ResourceTreeDto>>;
}
