using Core.Domain.Common;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Events.Employment
{
    
    public class CreateEmploymentEvent : IDomainEvent
    {
        public HR.Domain.Entities.Employment newEmployment { get; }
        public DateTime OccurredOn { get; }

        // ✅ نسخه اصلاح‌شده: استفاده مستقیم از DateTime.UtcNow
        public CreateEmploymentEvent(HR.Domain.Entities.Employment _newEmployment)
        {
            newEmployment = _newEmployment;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
