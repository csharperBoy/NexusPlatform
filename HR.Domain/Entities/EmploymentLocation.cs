using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    /// <summary>
    /// مکان های مرتبط با کارمندان در سازمان
    /// </summary>
    public class EmploymentLocation : BaseEntity, IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        public Guid FkLocationId { get; private set; }
        public Guid FkEmployeeId { get; private set; }


        public virtual Employment Employee { get; set; } = null!;

        public virtual Location Location { get; set; } = null!;
        protected EmploymentLocation() { }
        public EmploymentLocation(
             Guid _fkLocationId,
             Guid _fkEmployeeId
            )
        {
            FkLocationId = _fkLocationId;
            FkEmployeeId = _fkEmployeeId;

        }

    }
}
