using Core.Shared.DTOs.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Navigation.PublicService
{
    public interface IMenuPublicService
    {
        Task SyncModuleMenusAsync(List<MenuDto> menus, CancellationToken cancellationToken);
    }
}
