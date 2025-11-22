using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Enums
{
    public enum ScopeType : byte
    {
        All = 1,
        Subtree = 2,  // خودش + زیرمجموعه‌ها (با CTE)
        Self = 3,
        Unit = 4,
        SpecificUnit = 5
    }
}
