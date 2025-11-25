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

        public ResourceType Type { get; init; }
        public ResourceCategory Category { get; init; }

        public Guid? ParentId { get; init; }
    }
}
