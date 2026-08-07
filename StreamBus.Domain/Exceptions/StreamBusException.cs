using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamBus.Domain.Exceptions
{
    public class StreamBusException : Exception
    {
        public StreamBusException(string message) : base(message) { }

        public StreamBusException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
