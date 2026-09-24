using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Domain.Enums
{
    public enum TokenStatus
    {
        Empty = 0,
        Valid = 1,
        Expired = 2,
        Invalid = 3,
    }
}
