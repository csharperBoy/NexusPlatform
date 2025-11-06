using Core.Domain.Specifications;
using Sample.Domain.Entities;

namespace Sample.Domain.Specifications
{
    public class SampleGetSpec : BaseSpecification<Entities.Sample>
    {
        public SampleGetSpec(string property1)
            : base(r => r.property1 == property1)
        {
            //AddInclude(r => r.User);
            //OrderBy(r=>r.property1);
            //OrderByDescending(r=>r.property1);
            //ApplyPaging(0, 10);
            //ApplyThenOrderBy(r => r.Id);
        }
    }
}
