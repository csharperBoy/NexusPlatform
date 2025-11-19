using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Events
{
    public class ResourceCreatedEvent : IDomainEvent
    {
        public Guid ResourceId { get; }
        public string Name { get; }
        public string Code { get; }
        public ResourceType Type { get; }
        public DateTime OccurredOn { get; }
        public ResourceCreatedEvent(Guid resourceId, string name, string code, ResourceType type)
        {
            ResourceId = resourceId;
            Name = name;
            Code = code;
            Type = type;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
}
