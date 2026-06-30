using Core.Domain.Common.EntityProperties;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class PermissionAssignee: BaseEntity ,IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        public AssigneeType Type { get; private set; }
        //navigate
        public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
        protected PermissionAssignee()
        {
            
        }
        public PermissionAssignee(AssigneeType _Type)
        {
            Type = _Type;
        }
    }
}
