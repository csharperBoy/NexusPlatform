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
        View = 0,
        Create = 1,
        Edit = 2,
        Delete =3,
        Export = 4,

        // دسترسی کامل
        Full = 99
    }
}
