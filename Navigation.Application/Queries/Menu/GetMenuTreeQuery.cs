using Core.Shared.DTOs.Base;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Enums.Authorization;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Application.Queries.Menu
{
    public record GetMenuTreeQuery(
       string? Title = null) : IRequest<Result<IList<MenuDto>>>;

}
