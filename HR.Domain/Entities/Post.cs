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
    public class Post : BaseEntity, IAuditableEntity, IAggregateRoot , IHierarchicalStructureEntity<Post, Guid?>
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion

        #region IHierarchicalStructureEntity Impelement
        public Guid? FkParentId { get; private set; }
        public virtual Post? Parent { get; private set; }
        public virtual ICollection<Post> Children { get; private set; } = new List<Post>();
        public void ChangeParent(Guid? newParentId)
        {
            if (newParentId == Id)
                throw new InvalidOperationException("Menu cannot be its own parent.");

            FkParentId = newParentId;
            Touch();

            // ارسال ایونت وقتی ساختار سلسله مراتب تغییر می‌کند
            //AddDomainEvent(new MenuHierarchyChangedEvent(Id));
        }
        #endregion

        private void Touch() => ModifiedAt = DateTime.UtcNow;

        public string Code { get; private set; }
        public Guid FkOrganizationUnitId { get; private set; }
        public Guid FkJobTitleId { get; private set; }
        public Guid? FkJobLevelId { get; private set; }
        public Guid? FkGradeId { get; private set; }
        public Guid? FkCostCenterId { get; private set; }
        public bool IsActive { get; private set; }


        public Guid FkPermissionAssigneeId { get; set; }
        // Navigation

        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public virtual CostCenter? CostCenter { get; set; }

        public virtual Grade? Grade { get; set; }

        public virtual JobLevel? JobLevel { get; set; }

        public virtual JobTitle JobTitle { get; set; } = null!;

        public virtual OrganizationUnit OrganizationUnit { get; set; } = null!;





        protected Post() { }

        public Post(
            string _Code,
            Guid _OrganizationUnitId,
            Guid _JobTitleId,
            Guid? _JobLevelId = null,
            Guid? _GradeId = null,
            Guid? _CostCenterId = null,
            Guid? _parentId = null
            )
        {
            Code = _Code;
            FkOrganizationUnitId = _OrganizationUnitId;
            FkJobTitleId = _JobTitleId;
            FkJobLevelId = _JobLevelId;
            FkGradeId = _GradeId;
            FkCostCenterId = _CostCenterId;
            FkParentId = _parentId;
        }
    }

}
