using Authorization.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Resource
{
    /*
     📌 ResourceTreeDto
     --------------------
     ساختار درختی Resource جهت:
     - نمایش منو
     - نمایش ساختار سطوح دسترسی
     - ساختار سلسله‌مراتبی

     شامل Children بازگشتی.
    */

    public class ResourceTreeDto
    {
        public Guid Id { get; init; }
        public string Key { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public ResourceType Type { get; init; }
        public ResourceCategory Category { get; init; }
        public Guid? ParentId { get; init; }
        public string ParentKey { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public int DisplayOrder { get; init; }
        public string Icon { get; init; } = string.Empty;
        public string Route { get; init; } = string.Empty;
        public string Path { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public DateTime? ModifiedAt { get; init; }
        public string? ModifiedBy { get; init; }

        public IReadOnlyList<ResourceTreeDto> Children { get; init; } = Array.Empty<ResourceTreeDto>();
    }
}
