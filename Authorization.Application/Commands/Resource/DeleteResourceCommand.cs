using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.Resource
{
    /*
     📌 DeleteResourceCommand
     ------------------------
     Command برای حذف Resource.
    */
    public record DeleteResourceCommand(
        Guid Id
    ) : IRequest<Result<bool>>;
}
