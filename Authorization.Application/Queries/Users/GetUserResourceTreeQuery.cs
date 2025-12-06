using Authorization.Application.DTOs.Resource;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.Users
{
    public record GetUserResourceTreeQuery(Guid UserId)
        : IRequest<Result<IReadOnlyList<ResourceTreeDto>>>;
}
