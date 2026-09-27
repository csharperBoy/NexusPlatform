using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Domain.Enums
{
    public enum SchedulePlanStatus
    {
        Idle = 0,
        Waiting = 1,
        LoggingIn = 2,
        Refreshing = 3,
        WaitingFire = 4,
        Firing = 5,
        Done = 6,
        Cancelled = 7,
        Error = 8,
    }
}
