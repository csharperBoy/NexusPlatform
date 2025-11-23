using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Events
{
    /*
    📌 PermissionAssignedEvent
    ---------------------------
    هنگام ثبت یا تغییر Permission اجرا می‌شود.
    */

    public class PermissionAssignedEvent : IDomainEvent
    {
        public Permission Permission { get; }
        public DateTime OccurredOn { get; }

        public PermissionAssignedEvent(Permission permission)
        {
            Permission = permission;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
