using Core.Application.Abstractions.Base.PublicService;
using Core.Shared.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Application.Interfaces.Service
{
    public interface IMenuInternalService : IMenuPublicService
    {
        Task<IReadOnlyList<MenuDto>> GetMenus(string? title);
        Task<IReadOnlyList<MenuDto>> GetByTreeStructure(Guid? RootId = null);
    }
}
