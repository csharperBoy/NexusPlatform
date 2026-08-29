using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Domain.Events
{

    public class ChangeNaturalPersonEvent : IDomainEvent
    {

        public Guid Id { get; }


        public ChangeNaturalPersonEvent(

    Guid Id
            )
        {
            this.Id = Id;
        }

        public DateTime OccurredOn => DateTime.UtcNow;
    }
}
