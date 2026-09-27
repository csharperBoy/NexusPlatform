using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler.Application.Attributes
{
    /// <summary>
    /// کلید یکتای یک payload. برای serialization/deserialization استفاده میشه.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ScheduledJobAttribute : Attribute
    {
        public string Key { get; }

        public ScheduledJobAttribute(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be empty", nameof(key));
            Key = key;
        }
    }
}
