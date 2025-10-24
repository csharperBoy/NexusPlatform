using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Domain.Events
{
    public class PersonProfileUpdatedEvent : IDomainEvent
    {
        public Guid PersonId { get; }
        public Guid OldPersonProfileId { get; }
        public Guid NewPersonProfileId { get; }
        public int OldNumberOfChildren { get; }
        public int NewNumberOfChildren { get; }
        public DateTime EffectiveFrom { get; }
        public DateTime OccurredOn { get; }

        public PersonProfileUpdatedEvent(Guid personId, Guid oldPersonProfileId, Guid newPersonProfileId,
                                       int oldNumberOfChildren, int newNumberOfChildren, DateTime effectiveFrom)
        {
            PersonId = personId;
            OldPersonProfileId = oldPersonProfileId;
            NewPersonProfileId = newPersonProfileId;
            OldNumberOfChildren = oldNumberOfChildren;
            NewNumberOfChildren = newNumberOfChildren;
            EffectiveFrom = effectiveFrom;
            OccurredOn = DateTime.UtcNow;
        }
    }
}
