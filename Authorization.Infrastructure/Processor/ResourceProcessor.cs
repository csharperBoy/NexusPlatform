using Authorization.Application.DTOs.Resource;
using Authorization.Application.Interfaces;
using Authorization.Domain.Entities;
using Core.Shared.DTOs.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Processor
{
    public class ResourceProcessor: IResourceProcessor
    {
        public IReadOnlyList<ResourceTreeDto> BuildTree(IEnumerable<Resource> resources, Guid? parentId = null)
        {
            var nodes = resources
                .Where(r => r.ParentId == parentId)
                .OrderBy(r => r.DisplayOrder)
                .ThenBy(r => r.Name)
                .Select(resource => new ResourceTreeDto
                {
                    Id = resource.Id,
                    Key = resource.Key,
                    Name = resource.Name,
                    Description = resource.Description,
                    Type = resource.Type,
                    Category = resource.Category,
                    ParentId = resource.ParentId,
                    ParentKey = resource.Parent?.Key ?? string.Empty,
                    IsActive = resource.IsActive,
                    DisplayOrder = resource.DisplayOrder,
                    Icon = resource.Icon,
                    Path = resource.ResourcePath,
                    CreatedAt = resource.CreatedAt,
                    CreatedBy = resource.CreatedBy,
                    ModifiedAt = resource.ModifiedAt,
                    ModifiedBy = resource.ModifiedBy,
                    Children = BuildTree(resources, resource.Id)
                })
                .ToList();

            return nodes;
        }
        // الگوریتم Flatten کردن درخت
        public List<ResourceDto> FlattenResources(List<ResourceDto> resources)
        {
            var result = new List<ResourceDto>();
            foreach (var res in resources)
            {
                // والد اول اضافه می‌شود
                result.Add(res);

                // بعد فرزندان به صورت بازگشتی
                if (res.Children != null && res.Children.Any())
                {
                    // ست کردن ParentKey برای فرزندان (جهت اطمینان)
                    foreach (var child in res.Children) child.ParentKey = res.Key;

                    result.AddRange(FlattenResources(res.Children));
                }
            }
            return result;
        }

    }
}
