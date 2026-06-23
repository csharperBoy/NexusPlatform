using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    /// <summary>
    /// نوع کارمندان
    /// تمام‌وقت (FullTime)
    ///    پاره‌وقت(PartTime)
    ///پیمانی / پروژه‌ای(Contract / ProjectBased)
    ///کارآموزی(Internship)
    ///دورکاری(Remote)
    ///شیفتی(Shift)
    /// </summary>
    public class EmploymentType : BaseEntity
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        protected EmploymentType()
        {

        }
    }
}
