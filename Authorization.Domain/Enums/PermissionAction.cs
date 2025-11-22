using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Enums
{
    public enum PermissionAction : byte
    {
        View = 1,
        Add = 2,
        Edit = 3,
        Delete = 4,
        Full = 99  // همه دسترسی‌ها
    }
}
