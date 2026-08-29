using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Events.Post
{


    public class ChangePostEvent : IDomainEvent
    {

        public Guid Id { get; }


        public ChangePostEvent(

    Guid Id
            )
        {
            this.Id = Id;
        }

        public DateTime OccurredOn => DateTime.UtcNow;
    }
}
