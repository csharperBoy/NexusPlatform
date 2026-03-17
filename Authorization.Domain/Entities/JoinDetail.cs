using Core.Domain.Attributes;
using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{

    [SecuredResource("Authorization.JoinDetail")]
    public class JoinDetail : BaseEntity, IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        public Guid PermissionRuleId { get; private set; }
        public string JoinLocalKey { get; private set; }
        public string JoinForeignKey { get; private set; }
        public string JoinEntity { get; private set; }
    }
}
