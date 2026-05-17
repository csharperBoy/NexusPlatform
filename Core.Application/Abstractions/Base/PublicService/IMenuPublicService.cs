using Core.Shared.DTOs.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Base.PublicService
{
    public interface IMenuPublicService
    {
        Task SyncModuleMenusAsync(List<MenuDto> menus, CancellationToken cancellationToken);
    }
}
