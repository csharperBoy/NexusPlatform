using Core.Domain.Common;
using HR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Events.Employment
{

    public class RemoveEmploymentEvent : IDomainEvent
    {
        public Guid Id { get; }
        public string EmploymentCode { get; }
        public Guid FkNaturalPersonId { get; }
        public Guid? FkEmploymentTypeId { get; }
        public Guid? FkEmploymentStatusId { get; }
        public Guid FkContactProfileId { get; }



        public RemoveEmploymentEvent(Guid Id,
                     string EmploymentCode,
         Guid FkNaturalPersonId,
         Guid? FkEmploymentTypeId,
         Guid? FkEmploymentStatusId,
         Guid FkContactProfileId




            )
        {
            this.Id = Id;
            this.EmploymentCode = EmploymentCode;
            this.FkNaturalPersonId = FkNaturalPersonId;
            this.FkEmploymentTypeId = FkEmploymentTypeId;
            this.FkEmploymentStatusId = FkEmploymentStatusId;
            this.FkContactProfileId = FkContactProfileId;
        }

        public DateTime OccurredOn => DateTime.UtcNow;
    }
}
