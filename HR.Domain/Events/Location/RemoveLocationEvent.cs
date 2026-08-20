using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Events.Location
{
    
    public class RemoveLocationEvent : IDomainEvent
    {
        public Guid Id { get;  }

        public Guid ProfileId { get;  }
        public string Title { get; }


        public RemoveLocationEvent(
              Guid Id,
    Guid ProfileId,
         string Title
            )
        {
            this.Id = Id;
            this.ProfileId = ProfileId;
            this.Title = Title;
        }

        public DateTime OccurredOn => DateTime.UtcNow;
    }
}
