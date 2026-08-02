using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Core.Shared.Enums.Authorization;
using HR.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    
    public class Assignment : BaseEntity, IAuditableEntity , IHasEffectivePeriod
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        public void Touch() => ModifiedAt = DateTime.UtcNow;

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
        public Guid FkPostId { get; private set; }
        public Guid FkEmploymentId { get; private set; }
        //public Guid AssignmentTypeId { get; private set; }
        public PostAssignmentType AssigneeType { get; private set; }
      
        // navigate

        public virtual Post Post { get; private set; } = null!;
        public virtual Employment Employment { get; private set; } = null!;
        protected Assignment() { }
        public Assignment(Guid _PostId, Guid _EmploymentId, PostAssignmentType? _AssignmentType =null , DateTime? _EffectiveFrom = null, DateTime? _EffectiveTo = null)
        {
            FkPostId = _PostId;
            FkEmploymentId = _EmploymentId;
            AssigneeType = _AssignmentType ?? PostAssignmentType.Delegation;
            if (_EffectiveFrom == null)
            {
                _EffectiveFrom = DateTime.UtcNow;
            }
            EffectiveFrom =  _EffectiveFrom;
            EffectiveTo = _EffectiveTo;
            IsCurrent = true;
        }
        
    }
}
