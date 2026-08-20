using Contact.Domain.Enums;
using Core.Domain.Common.EntityProperties;
using Core.Shared.Enums.Contact;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Domain.Entities
{
   
    public class ContactProfileAssignment : BaseEntity, IAuditableEntity,  IHasEffectivePeriod
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



        public Guid ContactProfileId { get; private set; }
        public ContactProfile ContactProfile { get; private set; }

        public Guid ContactResourceId { get; private set; }
        public ContactResource ContactResource { get; private set; }

        protected ContactProfileAssignment() { }
        public ContactProfileAssignment
            (
            Guid _ContactProfileId,
            DateTime? _EffectiveFrom = null,            
            DateTime? _EffectiveTo = null,
            bool _isCurrent = true
            )
        {
            ContactProfileId = _ContactProfileId;
            EffectiveFrom = _EffectiveFrom;
            EffectiveTo = _EffectiveTo;
            IsCurrent = _isCurrent;
        }
    }
}
