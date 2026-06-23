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
    public class EmploymentLocations : BaseEntity, IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        public Guid fkLocationId { get; private set; }
        public Guid fkEmployeeId { get; private set; }

        public virtual Location location { get; private set; }
        public virtual Employment employee { get; private set; }
        protected EmploymentLocations() { }
        public EmploymentLocations(
             Guid _fkLocationId,
             Guid _fkEmployeeId
            )
        {
            fkLocationId = _fkLocationId;
            fkEmployeeId = _fkEmployeeId;

        }

    }
}
