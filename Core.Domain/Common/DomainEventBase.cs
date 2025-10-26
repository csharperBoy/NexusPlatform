using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Core.Domain.Common
{
    // Strong base for domain events
    public abstract class DomainEventBase : IDomainEvent
    {
        public DateTime OccurredOn { get; }

        protected DomainEventBase(DateTime? occurredOn = null)
        {
            OccurredOn = occurredOn ?? DateTime.UtcNow;
        }
    }
}