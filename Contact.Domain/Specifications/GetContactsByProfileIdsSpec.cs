using Contact.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Specifications
{
  
    //public class GetContactsByProfileIdsSpec : BaseSpecification<ContactResource>
    //{
    //    public GetContactsByProfileIdsSpec(List<Guid> profileIds, bool onlyCurrent = true)
    //        : base(resource => resource.Assignments.Any(a =>
    //            profileIds.Contains(a.ContactProfileId) &&
    //            (!onlyCurrent || a.IsCurrent)))
    //    {
    //        // در صورت نیاز به Include کردن اطلاعات والد (مثل شماره اصلی متصل به داخلی)
    //        AddInclude(r => r.ParentContactResource);

    //        // در صورت نیاز به Include کردن خود انتساب‌ها
    //        AddInclude(r => r.Assignments);
    //    }
    //}

    public class GetContactsByProfileIdsSpec : BaseSpecification<ContactProfileAssignment>
    {
        public GetContactsByProfileIdsSpec(List<Guid> profileIds, bool onlyCurrent = true)
            : base(assignment =>
                profileIds.Contains(assignment.ContactProfileId) &&
                (!onlyCurrent || assignment.IsCurrent))
        {
            // دریافت اطلاعات خود منبع ارتباطی
            AddInclude(a => a.ContactResource);
            AddInclude(a => a.ContactProfile);

            // دریافت منابع فرعی/فرزند (مثل داخلی‌ها یا کانال‌های وابسته)
            AddInclude(a => a.ContactResource.ChildContactResources);
        }
    }
}
