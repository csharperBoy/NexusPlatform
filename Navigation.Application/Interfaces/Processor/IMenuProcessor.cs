using Navigation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Shared.DTOs.Navigation;

namespace Navigation.Application.Interfaces.Processor
{
    public interface IMenuProcessor
    {
        IReadOnlyList<MenuDto> BuildTree(IEnumerable<Menu> menus, Guid? parentId = null);
        List<MenuDto> Flatten(List<MenuDto> resources);
    }
}
