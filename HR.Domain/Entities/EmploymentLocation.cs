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
    public class EmploymentLocation : BaseEntity, IAuditableEntity, IHasEffectivePeriod
    {
        #region IAuditableEntity Impelement
        public void Touch() => ModifiedAt = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

        #endregion
        #region Impelement IHasEffectivePeriod
        public DateTime? EffectiveFrom { get; private set; }
        public DateTime? EffectiveTo { get; private set; }
        public bool IsCurrent { get; private set; }

        public void SetEffectiveFrom(DateTime? value)
        {
            EffectiveFrom = value;
            Touch();
        }

        public async Task SetEffectiveTo(DateTime? value)
        {
            EffectiveTo = value;
            Touch();
            await Task.CompletedTask;
        }
        public async Task SetIsCurrent(bool value)
        {
            IsCurrent = value;
            Touch();
            await Task.CompletedTask;
        }
        #endregion
        public Guid FkLocationId { get; private set; }
        public Guid FkEmploymentId { get; private set; }


        public virtual Employment Employment { get; set; } = null!;

        public virtual Location Location { get; set; } = null!;
        
        protected EmploymentLocation() { }
        public EmploymentLocation(
             Guid _fkLocationId,
             Guid _fkEmploymentId
            )
        {
            FkLocationId = _fkLocationId;
            FkEmploymentId = _fkEmploymentId;

        }

    }
}
