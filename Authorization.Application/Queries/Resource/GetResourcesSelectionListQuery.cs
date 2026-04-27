using Authorization.Application.DTOs.Resource;
using Core.Shared.DTOs;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Queries.Resource
{
    public record GetResourcesSelectionListQuery() : IRequest<Result<IList<SelectionListDto>>>;

}
