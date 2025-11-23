using Authorization.Domain.Entities;
using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Events
{
    /*
     📌 DataScopeChangedEvent
     -------------------------
     هنگام ایجاد یا تغییر DataScope اجرا می‌شود.
     */

    public class DataScopeChangedEvent : IDomainEvent
    {
        public DataScope DataScope { get; }
        public DateTime OccurredOn { get; }

        public DataScopeChangedEvent(DataScope dataScope)
        {
            DataScope = dataScope;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
