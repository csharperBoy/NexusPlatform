using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    /// <summary>
    /// پست سازمانی (برای ساخت چارت سازمانی)
    /// </summary>
    public class Post : BaseEntity, IAuditableEntity, IAggregateRoot 
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        public string Code { get; private set; }
        public Guid OrganizationUnitId { get; private set; }
        public Guid JobTitleId { get; private set; }
        public Guid? JobLevelId { get; private set; }
        public Guid? GradeId { get; private set; }
        public Guid? CostCenterId { get; private set; }
        public Guid? ReportsToPostId { get; private set; }
        public bool IsActive { get; private set; }


        // Navigation
        public virtual OrganizationUnit OrganizationUnit { get; private set; } = null!;
        public virtual JobTitle JobTitle { get; private set; } = null!;
        public virtual JobLevel? JobLevel { get; private set; } = null!;
        public virtual Grade? Grade { get; private set; } = null!;
        public virtual CostCenter? CostCenter { get; private set; } = null!;
        public virtual Post? ReportsTo { get; private set; }
        public virtual ICollection<Assignment> Assignments { get; private set; } = new List<Assignment>();


        protected Post() { }

        public Post(
            string _Code,
            Guid _OrganizationUnitId,
            Guid _JobTitleId,
            Guid? _JobLevelId = null,
            Guid? _GradeId = null,
            Guid? _CostCenterId = null,
            Guid? _ReportsToPostId = null
            )
        {
            Code = _Code;
            OrganizationUnitId = _OrganizationUnitId;
            JobTitleId = _JobTitleId;
            JobLevelId = _JobLevelId;
            GradeId = _GradeId;
            CostCenterId = _CostCenterId;
            ReportsToPostId = _ReportsToPostId;

        }
    }

}
