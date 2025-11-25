using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
   
        public class AccessRule : AuditableEntity, IAggregateRoot
        {
            // Core Properties
            public string Name { get; private set; }
            public string Description { get; private set; }
            public RuleType Type { get; private set; }

            // Rule Conditions
            public string Condition { get; private set; } // شرط منطقی برای اجرای قانون
            public string Expression { get; private set; } // عبارت برای محاسبه نتیجه

            // Execution Settings
            public int ExecutionOrder { get; private set; }
            public bool StopOnMatch { get; private set; } = true;

            // Temporal Settings
            public DateTime? EffectiveFrom { get; private set; }
            public DateTime? ExpiresAt { get; private set; }

            // Metadata
            public bool IsActive { get; private set; } = true;
            public string Version { get; private set; } = "1.0";

            // Computed Properties
            public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;
            public bool IsEffective => !EffectiveFrom.HasValue || EffectiveFrom <= DateTime.UtcNow;
            public bool IsValid => IsActive && !IsExpired && IsEffective;

            // Navigation Properties
            public virtual ICollection<AccessRuleTarget> Targets { get; private set; } = new List<AccessRuleTarget>();

            // Constructor for EF Core
            protected AccessRule() { }

            // Main Constructor
            public AccessRule(
                string name,
                string description,
                RuleType type,
                string condition,
                string expression,
                int executionOrder = 0,
                bool stopOnMatch = true,
                DateTime? effectiveFrom = null,
                DateTime? expiresAt = null,
                string version = "1.0",
                string createdBy = "system")
            {
                ValidateInputs(name, condition, expression, executionOrder);

                Name = name.Trim();
                Description = description;
                Type = type;
                Condition = condition.Trim();
                Expression = expression.Trim();
                ExecutionOrder = executionOrder;
                StopOnMatch = stopOnMatch;
                EffectiveFrom = effectiveFrom;
                ExpiresAt = expiresAt;
                Version = version;
                CreatedBy = createdBy;
                CreatedAt = DateTime.UtcNow;

                AddDomainEvent(new AccessRuleCreatedEvent(Id, Name, Type));
            }

            // Domain Methods
            public void Update(
                string name,
                string description,
                string condition,
                string expression,
                int executionOrder,
                bool stopOnMatch,
                string version)
            {
                ValidateInputs(name, condition, expression, executionOrder);

                Name = name.Trim();
                Description = description;
                Condition = condition.Trim();
                Expression = expression.Trim();
                ExecutionOrder = executionOrder;
                StopOnMatch = stopOnMatch;
                Version = version;
                ModifiedAt = DateTime.UtcNow;

                AddDomainEvent(new AccessRuleUpdatedEvent(Id, Name, Condition, Expression));
            }

            public void AddTarget(Guid resourceId, TargetType targetType)
            {
                if (Targets.Any(t => t.ResourceId == resourceId && t.TargetType == targetType))
                    return;

                var target = new AccessRuleTarget(Id, resourceId, targetType);
                Targets.Add(target);

                AddDomainEvent(new AccessRuleTargetAddedEvent(Id, resourceId, targetType));
            }

            public void RemoveTarget(Guid resourceId, TargetType targetType)
            {
                var target = Targets.FirstOrDefault(t => t.ResourceId == resourceId && t.TargetType == targetType);
                if (target != null)
                {
                    Targets.Remove(target);
                    AddDomainEvent(new AccessRuleTargetRemovedEvent(Id, resourceId, targetType));
                }
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
                AddDomainEvent(new AccessRuleActivatedEvent(Id));
            }

            public void Deactivate()
            {
                if (!IsActive) return;

                IsActive = false;
                ModifiedAt = DateTime.UtcNow;
                AddDomainEvent(new AccessRuleDeactivatedEvent(Id));
            }

            // Validation Methods
            private static void ValidateInputs(string name, string condition, string expression, int executionOrder)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new AuthorizationDomainException("Access rule name cannot be empty.");

                if (string.IsNullOrWhiteSpace(condition))
                    throw new AuthorizationDomainException("Access rule condition cannot be empty.");

                if (string.IsNullOrWhiteSpace(expression))
                    throw new AuthorizationDomainException("Access rule expression cannot be empty.");

                if (executionOrder < 0)
                    throw new AuthorizationDomainException("Execution order cannot be negative.");
            }

            private static void ValidateTemporalRange(DateTime? effectiveFrom, DateTime? expiresAt)
            {
                if (effectiveFrom.HasValue && expiresAt.HasValue && effectiveFrom >= expiresAt)
                    throw new AuthorizationDomainException("Effective from date must be before expiration date.");
            }

            // Business Logic
            public bool AppliesToResource(Guid resourceId)
            {
                return Targets.Any(t => t.ResourceId == resourceId);
            }

            public bool ShouldExecute(IDictionary<string, object> context)
            {
                // در implementation کامل، شرط بر اساس context ارزیابی می‌شود
                return IsValid; // فعلا ساده
            }

            public object Evaluate(IDictionary<string, object> context)
            {
                // در implementation کامل، expression بر اساس context ارزیابی می‌شود
                return null; // فعلا ساده
            }
        }

       
}
