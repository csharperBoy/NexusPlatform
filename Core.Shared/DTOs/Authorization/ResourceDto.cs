using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using ResourceType = Core.Shared.Enums.Authorization.ResourceType;

namespace Core.Shared.DTOs.Authorization
{
    public class ResourceDto
    {
        public Guid Id { get; init; }
        public string Key { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public ResourceType Type { get; init; }
        public ResourceCategory Category { get; init; }
        public bool IsActive { get; init; }
        public int DisplayOrder { get; init; }
        public string Icon { get; init; } = string.Empty;
        public Guid? ParentId { get; init; }
        public string ParentKey { get; set; }
        public string Path { get; init; } = string.Empty;

        public List<ResourceDto> Children { get; set; } = new();

    }
}
