using Core.Shared.DTOs;
using Core.Shared.Results;
using Identity.Application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Queries.Role
{
    
    public record GetRolesSelectionListQuery(string? Name = null, string? description = null) : IRequest<Result<IList<SelectionListDto>>>;

}
