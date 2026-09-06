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
    public class Post : BaseEntity, IAuditableEntity, IAggregateRoot,ISoftRemovable, IHierarchicalStructureEntity<Post, Guid?>
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

       public void Touch() => ModifiedAt = DateTime.UtcNow;
        #endregion

        #region IHierarchicalStructureEntity Impelement
        public Guid? ParentId { get; private set; }
        public virtual Post? Parent { get; private set; }
        public virtual ICollection<Post> Children { get; private set; } = new List<Post>();
        public void ChangeParent(Guid? newParentId)
        {
            if (newParentId == Id)
                throw new InvalidOperationException("Menu cannot be its own parent.");

            ParentId = newParentId;
            Touch();

            // ارسال ایونت وقتی ساختار سلسله مراتب تغییر می‌کند
            //AddDomainEvent(new MenuHierarchyChangedEvent(Id));
        }
        #endregion
        #region ISoftRemovable Impelement
        public bool IsRemove { get; private set; } = false;

        public async Task SetIsRemove(bool value)
        {
            IsRemove = value;
            Touch();
            await Task.CompletedTask;
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

        public Guid FkContactProfileId { get; private set; }
        // Navigation

        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public virtual CostCenter? CostCenter { get; set; }

        public virtual Grade? Grade { get; set; }

        public virtual JobLevel? JobLevel { get; set; }

        public virtual JobTitle JobTitle { get; set; } = null!;

        public virtual OrganizationUnit? OrganizationUnit { get; set; } = null!;


        //public virtual ICollection<PostContact> PostContacts { get; set; } = new List<PostContact>();
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
            ParentId = parentId;
            Touch(); // به‌روزرسانی ModifiedAt
        }

        protected Post() { }

        public Post(
            string _Code,
            Guid _JobTitleId,
            Guid _FkContactProfileId,
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
            ParentId = _parentId;
            FkContactProfileId = _FkContactProfileId;
        }

        public bool ApplyChange(
                 Optional<string?> _Code ,
                 Optional<Guid> _FkJobTitleId ,
                 Optional<Guid?> _FkOrganizationUnitId ,
                 Optional<Guid?> _FkJobLevelId ,
                 Optional<Guid?> _FkGradeId ,
                 Optional<Guid?> _FkCostCenterId ,
                 Optional<bool?> _IsActive ,
            Optional<Guid?> _FkParentId

           )
        {
            bool hasChange = false;

            if (_Code.IsSet && _Code.Value?.Trim() != this.Code.Trim())
            {
                this.Code = _Code.Value;
                hasChange = true;
            }
            if (  _FkParentId.IsSet && _FkParentId.Value != this.ParentId)
            {
                this.ChangeParent(_FkParentId.Value);
                hasChange = true;
            }

            if (_FkJobTitleId.IsSet && _FkJobTitleId.Value != this.FkJobTitleId)
            {
                this.FkJobTitleId = _FkJobTitleId.Value;
                hasChange = true;
            }
            if (_FkOrganizationUnitId.IsSet && _FkOrganizationUnitId.Value != this.FkOrganizationUnitId)
            {
                this.FkOrganizationUnitId = _FkOrganizationUnitId.Value;
                hasChange = true;
            }
            if (_FkJobLevelId.IsSet && _FkJobLevelId.Value != this.FkJobLevelId)
            {
                this.FkJobLevelId = _FkJobLevelId.Value;
                hasChange = true;
            }
            if (_FkGradeId.IsSet && _FkGradeId.Value != this.FkGradeId)
            {
                this.FkGradeId = _FkGradeId.Value;
                hasChange = true;
            }
            if (_FkCostCenterId.IsSet && _FkCostCenterId.Value != this.FkCostCenterId)
            {
                this.FkCostCenterId = _FkCostCenterId.Value;
                hasChange = true;
            }
            if (_IsActive.IsSet && _IsActive.Value != this.IsActive)
            {
                this.IsActive = (bool)_IsActive.Value;
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
