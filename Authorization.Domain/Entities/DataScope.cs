using Authorization.Domain.Enums;
using Authorization.Domain.Events;
using Core.Domain.Common;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    /*
        public class DataScope : AuditableEntity, IAggregateRoot
        {
            public Guid ResourceId { get; private set; }
            public AssigneeType AssigneeType { get; private set; }
            public Guid AssigneeId { get; private set; }
            public ScopeType Scope { get; private set; }

            public Guid? SpecificUnitId { get; private set; }
            public string CustomFilter { get; private set; }
            public int Depth { get; private set; } = 1;

            public DateTime? EffectiveFrom { get; private set; }
            public DateTime? ExpiresAt { get; private set; }

            public string Description { get; private set; }
            public bool IsActive { get; private set; } = true;

        // Computed Properties
        [NotMapped]
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
        [NotMapped]
        public bool IsEffective => !EffectiveFrom.HasValue || EffectiveFrom <= DateTime.UtcNow;
        [NotMapped]
        public bool IsValid => IsActive && !IsExpired && IsEffective;

            // Navigation Properties
            public virtual Resource Resource { get; private set; } = null!;

            protected DataScope() { }

            public DataScope(
                Guid resourceId,
                AssigneeType assigneeType,
                Guid assigneeId,
                ScopeType scope,
                Guid? specificUnitId = null,
                string customFilter = "",
                int depth = 1,
                DateTime? effectiveFrom = null,
                DateTime? expiresAt = null,
                string description = "",
                string createdBy = "system")
            {
                if (resourceId == Guid.Empty) throw new ArgumentException("Resource ID cannot be empty.");
                if (assigneeId == Guid.Empty) throw new ArgumentException("Assignee ID cannot be empty.");
                ValidateScope(scope, specificUnitId, depth);

                ResourceId = resourceId;
                AssigneeType = assigneeType;
                AssigneeId = assigneeId;
                Scope = scope;
                SpecificUnitId = specificUnitId;
                CustomFilter = customFilter?.Trim();
                Depth = depth;
                EffectiveFrom = effectiveFrom;
                ExpiresAt = expiresAt;
                Description = description;
                CreatedBy = createdBy;
                CreatedAt = DateTime.UtcNow;
            }

            public void UpdateScope(
                ScopeType scope,
                Guid? specificUnitId,
                string customFilter,
                int depth,
                string description)
            {
                ValidateScope(scope, specificUnitId, depth);

                Scope = scope;
                SpecificUnitId = specificUnitId;
                CustomFilter = customFilter?.Trim();
                Depth = depth;
                Description = description;
                ModifiedAt = DateTime.UtcNow;

                // ارسال ایونت وقتی محدوده داده تغییر می‌کند
                AddDomainEvent(new DataScopeChangedEvent(AssigneeId, ResourceId));
            }

            public void SetTemporalRange(DateTime? effectiveFrom, DateTime? expiresAt)
            {
                if (effectiveFrom.HasValue && expiresAt.HasValue && effectiveFrom >= expiresAt)
                    throw new ArgumentException("Effective from date must be before expiration date.");

                EffectiveFrom = effectiveFrom;
                ExpiresAt = expiresAt;
                ModifiedAt = DateTime.UtcNow;
            }

            public void Activate()
            {
                if (IsActive) return;
                IsActive = true;
                ModifiedAt = DateTime.UtcNow;
            }

            public void Deactivate()
            {
                if (!IsActive) return;
                IsActive = false;
                ModifiedAt = DateTime.UtcNow;
            }

            private static void ValidateScope(ScopeType scope, Guid? specificUnitId, int depth)
            {
                if (scope == ScopeType.SpecificUnit && !specificUnitId.HasValue)
                    throw new ArgumentException("SpecificUnitId is required for SpecificUnit scope.");

                if (scope != ScopeType.SpecificUnit && specificUnitId.HasValue)
                    throw new ArgumentException("SpecificUnitId should only be set for SpecificUnit scope.");

                if (depth < 1 || depth > 10)
                    throw new ArgumentException("Depth must be between 1 and 10.");
            }
        [NotMapped]
        public bool RequiresSpecificUnit => Scope == ScopeType.SpecificUnit;
        [NotMapped]
        public bool IsHierarchical => Scope == ScopeType.Subtree;
        [NotMapped]
        public bool IsUnrestricted => Scope == ScopeType.All;

            public bool AppliesTo(AssigneeType assigneeType, Guid assigneeId)
            {
                return AssigneeType == assigneeType && AssigneeId == assigneeId;
            }
        }
    */
}
