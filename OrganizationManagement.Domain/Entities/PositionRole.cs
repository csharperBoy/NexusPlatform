using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrganizationManagement.Domain.Entities
{
    public class PositionRole : AuditableEntity, IAggregateRoot
    {
        public Guid FkPositionId { get; private set; }
        public Guid FkRoleId { get; private set; }

        // Navigation
        public virtual Position Position { get; private set; } = null!;
        //public virtual ApplicationRole Role { get; private set; } = null!;

        protected PositionRole() { }

        public PositionRole(Guid positionId, Guid roleId, string createdBy)
        {
            FkPositionId = positionId;
            FkRoleId = roleId;
            CreatedBy = createdBy;
        }
    }
}
