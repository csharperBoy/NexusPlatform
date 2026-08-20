using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Events.Post
{

    public class RemovePostEvent : IDomainEvent
    {
        public Guid Id { get; }

        public string Code { get; }
        public bool IsActive { get; }


        public Guid FkPermissionAssigneeId { get; }

        public Guid FkContactProfileId { get; }


        public RemovePostEvent(
               Guid Id,
    string Code,
         bool IsActive,
    Guid FkPermissionAssigneeId,
    Guid FkContactProfileId
            )
        {
            this.Id = Id;
            this.Code = Code;
            this.IsActive = IsActive;
            this.FkPermissionAssigneeId = FkPermissionAssigneeId;
            this.FkContactProfileId = FkContactProfileId;
        }

        public DateTime OccurredOn => DateTime.UtcNow;
    }
}
