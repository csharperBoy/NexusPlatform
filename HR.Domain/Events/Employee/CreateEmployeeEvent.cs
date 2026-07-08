using Core.Domain.Common;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Events.Employee
{
    
    public class CreateEmployeeEvent : IDomainEvent
    {
        public Employment newEmployee { get; }
        public DateTime OccurredOn { get; }

        // ✅ نسخه اصلاح‌شده: استفاده مستقیم از DateTime.UtcNow
        public CreateEmployeeEvent(Employment _newEmployee)
        {
            newEmployee = _newEmployee;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
