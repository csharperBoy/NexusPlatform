using Navigation.Application.Interfaces.Processor;
using Navigation.Domain.Entities;
using Core.Shared.DTOs.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Shared.Enums.Navigation;
using Core.Shared.Enums;
using Core.Shared.DTOs.Navigation;

namespace Navigation.Infrastructure.Processor
{
    public class MenuProcessor : IMenuProcessor
    {
        public IReadOnlyList<MenuDto> BuildTree(IEnumerable<Menu> menus, Guid? parentId = null)
        {
            var nodes = menus
                .Where(r => r.FkParentId == parentId)
                .OrderBy(r => r.Order)
                .ThenBy(r => r.Title)
                .Select(menu => new MenuDto
                {                           
                    Id = menu.Id,
                    Title = menu.Title,
                    Order = menu.Order,
                    Path = menu.Path,
                    Icon =  menu.Icon.GetIconString(),
                    Description = menu.Description,
                    ParentId = menu.FkParentId,
                    Key = menu.Key,
                    ParentKey = menu.Parent?.Key,
                    Children = BuildTree(menus, menu.Id)
                })                                      
                .ToList();

            return nodes;
        }
        // الگوریتم Flatten کردن درخت
        public List<MenuDto> Flatten(List<MenuDto> resources)
        {
            var result = new List<MenuDto>();
            foreach (var res in resources)
            {
                // والد اول اضافه می‌شود
                result.Add(res);

                // بعد فرزندان به صورت بازگشتی
                if (res.Children != null && res.Children.Any())
                {
                    // ست کردن ParentKey برای فرزندان (جهت اطمینان)
                    foreach (var child in res.Children) child.ParentKey = res.Key;

                    result.AddRange(Flatten(res.Children.ToList()));
                }
            }
            return result;
        }
    }
}
