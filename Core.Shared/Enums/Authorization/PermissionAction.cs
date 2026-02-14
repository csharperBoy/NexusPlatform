using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Enums.Authorization
{
    public enum PermissionAction : byte
    {
        // CRUD پایه
        View = 1,
        Create = 2,
        Edit = 3,
        Delete = 4,
        Export = 5,

        // دسترسی کامل
        Full = 99
    }
}
