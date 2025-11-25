using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class AccessRuleTarget : Entity
    {
        public Guid AccessRuleId { get; private set; }
        public Guid ResourceId { get; private set; }
        public TargetType TargetType { get; private set; }

        // Navigation Properties
        public virtual AccessRule AccessRule { get; private set; } = null!;
        public virtual Resource Resource { get; private set; } = null!;

        protected AccessRuleTarget() { }

        public AccessRuleTarget(Guid accessRuleId, Guid resourceId, TargetType targetType)
        {
            AccessRuleId = accessRuleId;
            ResourceId = resourceId;
            TargetType = targetType;
        }
    }

}
