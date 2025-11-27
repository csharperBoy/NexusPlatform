using Authorization.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Resource
{
    /*
     📌 ResourceDto
     ----------------
     مدل کامل Resource برای API و مدیریت.

     شامل اطلاعاتی است که برای نمایش یا ویرایش لازم است.
    */

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
        public string Route { get; init; } = string.Empty;
        public Guid? ParentId { get; init; }
        public string Path { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public DateTime? ModifiedAt { get; init; }
        public string? ModifiedBy { get; init; }
    }
}
