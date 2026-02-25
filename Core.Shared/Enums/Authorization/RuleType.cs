using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Enums.Authorization
{
    
    public enum RuleType : byte
    {

        Scope = 1,
        Field = 2,
        Relation = 3
    }
}
