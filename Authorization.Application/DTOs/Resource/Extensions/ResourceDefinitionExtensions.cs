using Authorization.Domain.Enums;
using Core.Application.Abstractions.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Resource.Extensions
{
    public static class ResourceDefinitionExtensions
    {
        public static ResourceDto ToResourceDto(this ResourceDefinition definition)
        {
            // تبدیل string به Enum
            if (!Enum.TryParse<ResourceType>(definition.Type, out var resourceType))
                resourceType = ResourceType.Ui;

            if (!Enum.TryParse<ResourceCategory>(definition.Category, out var resourceCategory))
                resourceCategory = ResourceCategory.General;

            return new ResourceDto
            {
                Key = definition.Key,
                Name = definition.Name,
                Description = definition.Description,
                Type = resourceType,
                Category = resourceCategory,
                DisplayOrder = definition.DisplayOrder,
                Icon = definition.Icon,
                Route = definition.Route,
                ParentKey = definition.ParentKey
            };
        }

        public static ResourceDefinition ToResourceDefinition(this ResourceDto dto)
        {
            return new ResourceDefinition
            {
                Key = dto.Key,
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type.ToString(),
                Category = dto.Category.ToString(),
                ParentKey = dto.ParentKey,
                DisplayOrder = dto.DisplayOrder,
                Icon = dto.Icon,
                Route = dto.Route,
                Permissions = Array.Empty<string>(),
                RequiredFeatures = Array.Empty<string>()
            };
        }
    }
}
