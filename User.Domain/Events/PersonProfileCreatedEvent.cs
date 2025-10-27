using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Domain.Events
{
    public class PersonProfileCreatedEvent : IDomainEvent
    {
        public Guid PersonId { get; }
        public Guid PersonProfileId { get; }
        public int NumberOfChildren { get; }
        public DateTime EffectiveFrom { get; }
        public DateTime OccurredOn { get; }

        public PersonProfileCreatedEvent(Guid personId, Guid personProfileId,
                                       int numberOfChildren, DateTime effectiveFrom)
        {
            PersonId = personId;
            PersonProfileId = personProfileId;
            NumberOfChildren = numberOfChildren;
            EffectiveFrom = effectiveFrom;
            OccurredOn = DateTime.UtcNow;
        }
    }
}