using Core.Domain.Attributes;
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

    [SecuredResource("Authorization.PermissionRule")]
    public class PermissionRule : BaseEntity, IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        public Guid PermissionId { get; private set; }


        // For Field
        public string? FieldName { get; private set; }

        
        public Guid? JoinDetailId {  get; private set; }

        public ComparisonOperator Operator { get; private set; } // = ، < ، > ، !=
        public string? Value { get; private set; }

        public LogicalOperator LogicalOperator { get; private set; } // AND / OR
        public int GroupOrder { get; private set; } // برای Nested Group
        public virtual JoinDetail? JoinDetail { get; private set; }
        public virtual Permission Permission { get; private set; } // navigation
        protected PermissionRule() { }

        public PermissionRule(Guid _PermissionId, string? _FieldName,  ComparisonOperator _Operator, string? _Value, LogicalOperator _LogicalOperator, int _GroupOrder,Guid? _JoinDetailId = null)
        {
            PermissionId = _PermissionId;
            FieldName = _FieldName;
            JoinDetailId = _JoinDetailId;
            Operator = _Operator;
            Value = _Value;
            LogicalOperator = _LogicalOperator;
            GroupOrder = _GroupOrder;
        }

        public bool ApplyChange(
            Guid? _permissionId,
            string? _fieldName,
            ComparisonOperator? _comparisonOperator,
            string? _value,
            LogicalOperator? _logicalOperator,
            int? _groupOrder)
        {
            bool hasChange = false;
            // آپدیت فیلدها
            if (_permissionId != null && _permissionId != this.PermissionId)
            {
                this.PermissionId = (Guid)_permissionId;
                hasChange = true;
            }
            if (_fieldName != null && _fieldName != this.FieldName)
            {
                this.FieldName = _fieldName;
                hasChange = true;
            }


            if (_value != null && _value != this.Value)
            {
                this.Value = _value;
                hasChange = true;
            }
            if (_logicalOperator != null && _logicalOperator != this.LogicalOperator)
            {
                this.LogicalOperator = (LogicalOperator)_logicalOperator;
                hasChange = true;
            }
            if (_groupOrder != null && _groupOrder != this.GroupOrder)
            {
                this.GroupOrder =(int)_groupOrder;
                hasChange = true;
            }
            
            if (hasChange)
            {
                Touch();
            }
            return hasChange;
        }
        private void Touch() => ModifiedAt = DateTime.UtcNow;

    }
}
