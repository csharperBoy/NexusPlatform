using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Domain.Entities
{
    public class PositionPerson : AuditableEntity, IAggregateRoot
    {
        public Guid FkPositionId { get; private set; }
        public Guid FkPersonId { get; private set; }

        // Navigation
        public virtual Position Position { get; private set; } = null!;

        protected PositionPerson() { }

        public PositionPerson(Guid positionId, Guid personId, string createdBy)
        {
            FkPositionId = positionId;
            FkPersonId = personId;
            CreatedBy = createdBy;
        }
    }
}
