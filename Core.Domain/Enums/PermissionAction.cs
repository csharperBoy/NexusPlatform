using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Enums
{
    public enum PermissionAction : byte
    {
        // CRUD پایه
        View = 1,
        Create = 2,
        Edit = 3,
        Delete = 4,

        // عملیات بیزینسی (مثال)
        Approve = 5,
        Reject = 6,
        Export = 7,

        // دسترسی کامل
        Full = 99
    }
}
