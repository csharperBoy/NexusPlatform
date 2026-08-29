using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Events
{
    public class ChangeContactProfileResourcesEvent : IDomainEvent
    {

        public Guid ProfileId { get; }


        public ChangeContactProfileResourcesEvent(

    Guid ProfileId
            )
        {
            this.ProfileId = ProfileId;
        }

        public DateTime OccurredOn => DateTime.UtcNow;
    }
}
