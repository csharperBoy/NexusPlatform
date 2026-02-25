using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Entities
{
    public class PermissionRule : BaseEntity, IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        public Guid PermissionId { get; private set; }

        public RuleType Type { get; private set; }
        // Scope
        // Field
        // Relation

        // For Field
        public string? FieldName { get; private set; }

        // For Relation
        public string? JoinEntity { get; private set; }
        public string? JoinLocalKey { get; private set; }
        public string? JoinForeignKey { get; private set; }
        public string? JoinField { get; private set; }

        public ComparisonOperator Operator { get; private set; }
        public string? Value { get; private set; }

        public LogicalOperator LogicalOperator { get; private set; } // AND / OR
        public int GroupOrder { get; private set; } // برای Nested Group
    }
}
