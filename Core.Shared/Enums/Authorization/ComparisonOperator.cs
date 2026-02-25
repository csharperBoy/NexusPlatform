using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Enums.Authorization
{
    public enum ComparisonOperator : byte
    {
        Equal = 1,
        GreateThan = 2,
        LessThan = 3,
        NotEqual = 4
    }
}
