using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    /// <summary>
    /// وضعیت کارمندان
    /// فعال / در حال خدمت (Active)
    ///در دوره‌ی آزمایشی(Probation)
    ///مرخصی بدون حقوق(LeaveOfAbsence)
    ///استعفا داده(Resigned)
    ///اخراج / تعدیل شده(Terminated)
    ///بازنشسته(Retired)
    ///در حال انتقال(Transferring)
    /// </summary>
    public class EmploymentStatus : BaseEntity
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        protected EmploymentStatus() { }
    }
}
