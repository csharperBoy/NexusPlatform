using Core.Shared.DTOs.Base;
using Base.Domain.Entities;
using Core.Shared.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Application.Interfaces.Processor
{
    public interface IMenuProcessor
    {
        IReadOnlyList<MenuDto> BuildTree(IEnumerable<Menu> menus, Guid? parentId = null);
        List<MenuDto> Flatten(List<MenuDto> resources);
    }
}
