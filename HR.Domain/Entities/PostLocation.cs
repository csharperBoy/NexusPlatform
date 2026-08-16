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
    public class PostLocation : BaseEntity, IAuditableEntity, IHasEffectivePeriod
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

        public async Task SetEffectiveFrom(DateTime? value)
        {
            EffectiveFrom = value;
            Touch();
            await Task.CompletedTask;
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
        public Guid FkPostId { get; private set; }


        public virtual Post Post { get; set; } = null!;

        public virtual Location Location { get; set; } = null!;

        protected PostLocation() { }
        public PostLocation(
             Guid _fkLocationId,
             Guid _fkPostId
            )
        {
            FkLocationId = _fkLocationId;
            FkPostId = _fkPostId;
            IsCurrent = true;
            EffectiveFrom = DateTime.UtcNow;
        }

    }
}
