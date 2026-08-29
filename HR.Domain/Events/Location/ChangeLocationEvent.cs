using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Events.Location
{
 
    public class ChangeLocationEvent : IDomainEvent
    {

        public Guid Id { get; }


        public ChangeLocationEvent(

    Guid Id
            )
        {
            this.Id = Id;
        }

        public DateTime OccurredOn => DateTime.UtcNow;
    }
}
