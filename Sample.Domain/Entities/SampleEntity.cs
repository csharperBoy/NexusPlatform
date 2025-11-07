using Core.Domain.Common;

namespace Sample.Domain.Entities
{
    public class SampleEntity : AuditableEntity
    {
        public string property1 { get; set; } = default!;

        // مثال از تولید رویداد دامنه
        public void MarkSample(string value)
        {
            property1 = value;
            AddDomainEvent(new Sample.Domain.Events.SampleActionEvent(value));
        }
    }

}
