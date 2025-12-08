using Authorization.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Resource
{
    public class ResourceRegistrationDto
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ResourceType Type { get; set; }
        public ResourceCategory Category { get; set; }
        public string ParentKey { get; set; } // کلید والد (اختیاری)
        public int DisplayOrder { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
        public string[] Permissions { get; set; } // دسترسی‌های پیش‌فرض
        public string[] RequiredFeatures { get; set; } // ویژگی‌های مورد نیاز
    }
}
