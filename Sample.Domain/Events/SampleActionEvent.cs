using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Domain.Events
{
    public class SampleActionEvent : IDomainEvent
    {
        public string ActionProperty1 { get; }
        public DateTime sampleActionTime { get; }

        public SampleActionEvent(string _ActionProperty1)
        {
            ActionProperty1 = _ActionProperty1;
            sampleActionTime = DateTime.UtcNow;
        }

        public DateTime OccurredOn => sampleActionTime;
    }
}