using Core.Shared.DTOs.Base;
using Base.Application.Interfaces.Service;
using Base.Application.Queries.Menu;
using Core.Shared.DTOs.Authorization;
using Core.Shared.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Application.Handlers.Queries.Menu
{
    public class GetMenuTreeQueryHandler : IRequestHandler<GetMenuTreeQuery, Result<IList<MenuDto>>>
    {
        private readonly IMenuInternalService _menuService;
        public GetMenuTreeQueryHandler(IMenuInternalService menuService)
            => _menuService = menuService;

        public async Task<Result<IList<MenuDto>>> Handle(GetMenuTreeQuery request, CancellationToken ct)
        {
            IReadOnlyList<MenuDto> menus = await _menuService.GetByTreeStructure();
            return Result<IList<MenuDto>>.Ok(menus.ToList());
        }
    }

}
