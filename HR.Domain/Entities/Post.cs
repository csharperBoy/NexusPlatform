using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Core.Shared.Enums.Authorization;
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
    public class Post : BaseEntity, IAuditableEntity, IAggregateRoot, IHierarchicalStructureEntity<Post, Guid?>
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

       public void Touch() => ModifiedAt = DateTime.UtcNow;
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


        public string Code { get; private set; }
        public Guid FkJobTitleId { get; private set; }
        public Guid? FkOrganizationUnitId { get; private set; }
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

        public virtual OrganizationUnit? OrganizationUnit { get; set; } = null!;


        public virtual ICollection<PostContact> PostContacts { get; set; } = new List<PostContact>();
        public virtual ICollection<PostLocation> PostLocations { get; set; } = new List<PostLocation>();

        public void UpdateDetails(
    Guid? organizationUnitId,
    Guid? jobLevelId,
    Guid? gradeId = null,
    Guid? costCenterId = null,
    Guid? parentId = null)
        {
            FkOrganizationUnitId = organizationUnitId;
            FkJobLevelId = jobLevelId;
            FkGradeId = gradeId;
            FkCostCenterId = costCenterId;
            FkParentId = parentId;
            Touch(); // به‌روزرسانی ModifiedAt
        }

        protected Post() { }

        public Post(
            string _Code,
            Guid _JobTitleId,
            Guid? _OrganizationUnitId,
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

        public bool ApplyChange(
                 string? _Code = null,
                 Guid? _FkJobTitleId = null,
                 Guid? _FkOrganizationUnitId = null,
                 Guid? _FkJobLevelId = null,
                 Guid? _FkGradeId = null,
                 Guid? _FkCostCenterId = null,
                 bool? _IsActive = null,
            Guid? _FkParentId = null

           )
        {
            bool hasChange = false;

            if (_Code.Trim() != null && _Code.Trim() != this.Code.Trim())
            {
                this.Code = _Code;
                hasChange = true;
            }
            if ( _FkParentId != this.FkParentId)
            {
                this.ChangeParent(_FkParentId);
                hasChange = true;
            }

            if (_FkJobTitleId != null && _FkJobTitleId != this.FkJobTitleId)
            {
                this.FkJobTitleId = (Guid)_FkJobTitleId;
                hasChange = true;
            }
            if (_FkOrganizationUnitId != null && _FkOrganizationUnitId != this.FkOrganizationUnitId)
            {
                this.FkOrganizationUnitId = (Guid)_FkOrganizationUnitId;
                hasChange = true;
            }
            if (_FkJobLevelId != null && _FkJobLevelId != this.FkJobLevelId)
            {
                this.FkJobLevelId = (Guid)_FkJobLevelId;
                hasChange = true;
            }
            if (_FkGradeId != null && _FkGradeId != this.FkGradeId)
            {
                this.FkGradeId = _FkGradeId;
                hasChange = true;
            }
            if (_FkCostCenterId != null && _FkCostCenterId != this.FkCostCenterId)
            {
                this.FkCostCenterId = _FkCostCenterId;
                hasChange = true;
            }
            if (_IsActive != null && _IsActive != this.IsActive)
            {
                this.IsActive = (bool)_IsActive;
                hasChange = true;
            }
           
            if (hasChange)
            {
                Touch();
            }
            return hasChange;
        }

    }

}
