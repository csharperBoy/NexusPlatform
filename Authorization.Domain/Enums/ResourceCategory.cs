using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Enums
{
    public enum ResourceCategory : byte
    {
        Module = 1,
        Menu = 2,
        Page = 3,  // خودش + زیرمجموعه‌ها (با CTE)
        Component = 4,
        DatabaseTable = 5
    }
}
