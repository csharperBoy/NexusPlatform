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
    
    public class Assignment : BaseEntity, IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        public Guid PostId { get; private set; }
        public Guid EmploymentId { get; private set; }
        //public Guid AssignmentTypeId { get; private set; }
        public PostAssignmentType AssigneeType { get; private set; }
        public DateOnly EffectiveFrom { get; private set; }
        public DateOnly? EffectiveTo { get; private set; }
        public bool IsCurrent { get; private set; }
        public virtual Post Post { get; private set; } = null!;
        protected Assignment() { }
        public Assignment(Guid _PostId, Guid _EmploymentId, PostAssignmentType? _AssignmentType =null , DateOnly? _EffectiveFrom = null, DateOnly? _EffectiveTo = null)
        {
            PostId = _PostId;
            EmploymentId = _EmploymentId;
            AssigneeType = _AssignmentType ?? PostAssignmentType.Delegation;
            if (_EffectiveFrom == null)
            {
                _EffectiveFrom = DateOnly.FromDateTime(DateTime.UtcNow);
            }
            EffectiveFrom = (DateOnly) _EffectiveFrom;
            EffectiveTo = _EffectiveTo;
        }
    }
}
