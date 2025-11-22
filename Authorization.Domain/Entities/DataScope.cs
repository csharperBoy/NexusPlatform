using Authorization.Domain.Enums;
using Authorization.Domain.ValueObjects;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class DataScope : AuditableEntity, IAggregateRoot
    {
        public Guid ResourceId { get; private set; }
        public AssigneeType AssigneeType { get; private set; }
        public Guid AssigneeId { get; private set; }
        public ScopeType Scope { get; private set; }
        public Guid? SpecificUnitId { get; private set; }  // فقط برای SpecificUnit

        // Navigation
        public virtual Resource Resource { get; private set; } = null!;

        protected DataScope() { }  // EF Core

        public DataScope(Guid resourceId, AssigneeType assigneeType, Guid assigneeId,
                         ScopeType scope, Guid? specificUnitId = null, string createdBy = "system")
        {
            ResourceId = resourceId;
            AssigneeType = assigneeType;
            AssigneeId = assigneeId;
            Scope = scope;
            SpecificUnitId = specificUnitId;
            CreatedBy = createdBy;
        }

        // Domain Behavior
        public void UpdateScope(ScopeType newScope, Guid? newSpecificUnitId = null)
        {
            Scope = newScope;
            SpecificUnitId = newSpecificUnitId;
            ModifiedAt = DateTime.UtcNow;
        }
    }
}
