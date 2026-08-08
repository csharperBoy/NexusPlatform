using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamBus.Domain.Enums
{
    public enum StreamConnectionState
    {
        Disconnected = 0,
        Connecting = 1,
        Connected = 2,
        Faulted = 3
    }
}
