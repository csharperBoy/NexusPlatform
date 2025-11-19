using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Enums
{
    public enum PermissionLevel
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 3,
        Admin = 4
    }
}
