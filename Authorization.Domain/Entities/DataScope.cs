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
            // Core Properties
            public Guid ResourceId { get; private set; }
            public AssigneeType AssigneeType { get; private set; }
            public Guid AssigneeId { get; private set; }
            public ScopeType Scope { get; private set; }

            // Scope Specific Properties
            public Guid? SpecificUnitId { get; private set; }
            public string CustomFilter { get; private set; } // فیلتر سفارشی برای کوئری‌های پیچیده
            public int Depth { get; private set; } = 1; // عمق برای Subtree

            // Temporal Settings
            public DateTime? EffectiveFrom { get; private set; }
            public DateTime? ExpiresAt { get; private set; }

            // Metadata
            public string Description { get; private set; }
            public bool IsActive { get; private set; } = true;

            // Computed Properties
            public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
            public bool IsEffective => !EffectiveFrom.HasValue || EffectiveFrom <= DateTime.UtcNow;
            public bool IsValid => IsActive && !IsExpired && IsEffective;

            // Navigation Properties
            public virtual Resource Resource { get; private set; } = null!;

            // Constructor for EF Core
            protected DataScope() { }

            // Main Constructor
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
                ValidateInputs(resourceId, assigneeId, scope, specificUnitId, depth);

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

                AddDomainEvent(new DataScopeCreatedEvent(Id, ResourceId, AssigneeType, AssigneeId, Scope));
            }

            // Domain Methods
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

                AddDomainEvent(new DataScopeUpdatedEvent(Id, Scope, SpecificUnitId, CustomFilter));
            }

            public void SetTemporalRange(DateTime? effectiveFrom, DateTime? expiresAt)
            {
                ValidateTemporalRange(effectiveFrom, expiresAt);

                EffectiveFrom = effectiveFrom;
                ExpiresAt = expiresAt;
                ModifiedAt = DateTime.UtcNow;
            }

            public void Activate()
            {
                if (IsActive) return;

                IsActive = true;
                ModifiedAt = DateTime.UtcNow;
                AddDomainEvent(new DataScopeActivatedEvent(Id));
            }

            public void Deactivate()
            {
                if (!IsActive) return;

                IsActive = false;
                ModifiedAt = DateTime.UtcNow;
                AddDomainEvent(new DataScopeDeactivatedEvent(Id));
            }

            public void ChangeDepth(int newDepth)
            {
                if (newDepth < 1 || newDepth > 10)
                    throw new AuthorizationDomainException("Depth must be between 1 and 10.");

                Depth = newDepth;
                ModifiedAt = DateTime.UtcNow;
            }

            // Validation Methods
            private static void ValidateInputs(
                Guid resourceId,
                Guid assigneeId,
                ScopeType scope,
                Guid? specificUnitId,
                int depth)
            {
                if (resourceId == Guid.Empty)
                    throw new AuthorizationDomainException("Resource ID cannot be empty.");

                if (assigneeId == Guid.Empty)
                    throw new AuthorizationDomainException("Assignee ID cannot be empty.");

                ValidateScope(scope, specificUnitId, depth);
            }

            private static void ValidateScope(ScopeType scope, Guid? specificUnitId, int depth)
            {
                if (scope == ScopeType.SpecificUnit && !specificUnitId.HasValue)
                    throw new AuthorizationDomainException("SpecificUnitId is required for SpecificUnit scope.");

                if (scope != ScopeType.SpecificUnit && specificUnitId.HasValue)
                    throw new AuthorizationDomainException("SpecificUnitId should only be set for SpecificUnit scope.");

                if (depth < 1 || depth > 10)
                    throw new AuthorizationDomainException("Depth must be between 1 and 10.");
            }

            private static void ValidateTemporalRange(DateTime? effectiveFrom, DateTime? expiresAt)
            {
                if (effectiveFrom.HasValue && expiresAt.HasValue && effectiveFrom >= expiresAt)
                    throw new AuthorizationDomainException("Effective from date must be before expiration date.");
            }

            // Business Logic
            public bool RequiresSpecificUnit => Scope == ScopeType.SpecificUnit;
            public bool IsHierarchical => Scope == ScopeType.Subtree;
            public bool IsUnrestricted => Scope == ScopeType.All;

            public bool AppliesTo(AssigneeType assigneeType, Guid assigneeId)
            {
                return AssigneeType == assigneeType && AssigneeId == assigneeId;
            }

            public string GetScopeDescription()
            {
                return Scope switch
                {
                    ScopeType.All => "دسترسی به تمام داده‌ها",
                    ScopeType.Subtree => $"دسترسی سلسله مراتبی تا عمق {Depth}",
                    ScopeType.Self => "دسترسی فقط به داده‌های خود کاربر",
                    ScopeType.Unit => "دسترسی به داده‌های واحد سازمانی",
                    ScopeType.SpecificUnit => $"دسترسی به داده‌های واحد خاص: {SpecificUnitId}",
                    _ => "نوع محدوده نامشخص"
                };
            }
        }
    
}
