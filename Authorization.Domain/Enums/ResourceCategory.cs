using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Enums
{
    public enum ResourceCategory : byte
    {
        General = 0,
        System = 1,
        Module = 2,
        Menu = 3,
        Page = 4,  
        Component = 5,
        DatabaseTable = 6
    }
}
