using Core.Domain.Specifications;
using HR.IrisaSync.Extention.Entities;


namespace HR.IrisaSync.Extention.Specifications
{

    public class GetEmploymentSpec : BaseSpecification<PdsIdeaInformationViw>
    {
        public GetEmploymentSpec()
            : base(p =>
                         p.NumPrsnEmply == 868 || p.NumPrsnEmply == 867 || p.NumPrsnEmply == 310 || p.Id == null)
        {
        }
    }
}
