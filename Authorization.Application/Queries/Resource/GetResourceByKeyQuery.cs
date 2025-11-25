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
     📌 GetResourceByKeyQuery
     ------------------------
     Query برای دریافت Resource بر اساس Key
    */
    public record GetResourceByKeyQuery(string Key)
        : IRequest<Result<ResourceDto>>;
}
