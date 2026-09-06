using Core.Domain.Common;
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
            Optional<string?> _employmentCode ,
            Optional<Guid?> _employmentTypeId ,
            Optional<Guid?> _employmentStatusId ,
            Optional<DateOnly?> _startDate ,
            Optional<DateOnly?> _endDate 
            )
        {

            bool hasChange = false;

            if (_employmentCode.IsSet && _employmentCode.Value?.Trim() != EmploymentCode.Trim())
            {
                EmploymentCode = _employmentCode.Value;
                hasChange = true;
            }

            if (_employmentTypeId.IsSet && _employmentTypeId.Value != FkEmploymentTypeId)
            {
                FkEmploymentTypeId = _employmentTypeId.Value;
                hasChange = true;
            }

            if (_employmentStatusId.IsSet && _employmentStatusId.Value != FkEmploymentStatusId)
            {
                FkEmploymentStatusId = _employmentStatusId.Value;
                hasChange = true;
            }
            if (_startDate.IsSet && _startDate.Value != this.EffectiveFrom)
            {
                this.EffectiveFrom = _startDate.Value ?? DateOnly.FromDateTime(DateTime.UtcNow);
                hasChange = true;
            }
            if (_endDate.IsSet && _endDate.Value != this.EffectiveTo)
            {
                this.EffectiveTo = _endDate.Value;
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

