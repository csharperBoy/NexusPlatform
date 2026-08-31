using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{

    public class Employment : BaseEntity, IAuditableEntity, ISoftRemovable
    {

        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

        public void Touch() => ModifiedAt = DateTime.UtcNow;
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
        public string EmploymentCode { get; private set; }
        public Guid FkNaturalPersonId { get; private set; }
        public Guid? FkEmploymentTypeId { get; private set; }
        public Guid? FkEmploymentStatusId { get; private set; }
        public DateOnly EffectiveFrom { get; private set; }
        public DateOnly? EffectiveTo { get; private set; }
        public Guid FkContactProfileId { get; private set; }
        //navigate
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public virtual ICollection<EmploymentLocation> EmploymentLocations { get; set; } = new List<EmploymentLocation>();

        public virtual EmploymentStatus? EmploymentStatus { get; set; }

        public virtual EmploymentType? EmploymentType { get; set; }



        //public virtual ICollection<EmploymentContact> EmploymentContacts { get; set; } = new List<EmploymentContact>();
        protected Employment()
        {

        }
        public Employment(
              string _EmploymentCode,
         Guid _PersonId,
         Guid _FkContactProfileId,
         Guid? _EmploymentTypeId,
         Guid? _EmploymentStatusId,
         DateOnly? _EffectiveFrom = null,
         DateOnly? _EffectiveTo = null
            )
        {
            EmploymentCode = _EmploymentCode;
            FkNaturalPersonId = _PersonId;
            FkEmploymentTypeId = _EmploymentTypeId;
            FkEmploymentStatusId = _EmploymentStatusId;
            FkContactProfileId = _FkContactProfileId;
            EffectiveFrom = _EffectiveFrom ?? DateOnly.FromDateTime(DateTime.UtcNow);

            EffectiveTo = _EffectiveTo;

        }

        public Employment(string? _EmploymentCode, Guid? _FkEmploymentTypeId, Guid? _FkEmploymentStatusId, DateOnly? _EffectiveFrom,
         DateOnly? _EffectiveTo)
        {
            EmploymentCode = _EmploymentCode;
            FkEmploymentTypeId = _FkEmploymentTypeId;
            FkEmploymentStatusId = _FkEmploymentStatusId;
            EffectiveFrom = _EffectiveFrom ?? DateOnly.FromDateTime(DateTime.UtcNow);
            EffectiveTo = _EffectiveTo;

        }

        public bool ApplyChange(
            string? _employmentCode = null,
            Guid? _employmentTypeId = null,
            Guid? _employmentStatusId = null,
            DateOnly? _startDate = null,
            DateOnly? _endDate = null
            )
        {

            bool hasChange = false;

            if (_employmentCode != null && _employmentCode?.Trim() != EmploymentCode.Trim())
            {
                EmploymentCode = _employmentCode;
                hasChange = true;
            }

            if (_employmentTypeId != null && _employmentTypeId != FkEmploymentTypeId)
            {
                FkEmploymentTypeId = (Guid)_employmentTypeId;
                hasChange = true;
            }

            if (_employmentStatusId != null && _employmentStatusId != FkEmploymentStatusId)
            {
                FkEmploymentStatusId = (Guid)_employmentStatusId;
                hasChange = true;
            }
            if (_startDate != null && _startDate != this.EffectiveFrom)
            {
                this.EffectiveFrom = (DateOnly)_startDate;
                hasChange = true;
            }
            if (_endDate != null && _endDate != this.EffectiveTo)
            {
                this.EffectiveTo = (DateOnly)_endDate;
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

