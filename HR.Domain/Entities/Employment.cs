using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{

    public class Employment : BaseEntity , IAuditableEntity
    {

        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

        public void Touch() => ModifiedAt = DateTime.UtcNow;
        #endregion


        public string EmployeeCode { get; private set; }
        public Guid FkNaturalPersonId { get; private set; }
        public Guid? FkEmploymentTypeId { get; private set; }
        public Guid? FkEmploymentStatusId { get; private set; }
        public DateOnly EffectiveFrom { get; private set; }
        public DateOnly? EffectiveTo { get; private set; }

        //navigate
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public virtual ICollection<EmploymentLocation> EmploymentLocations { get; set; } = new List<EmploymentLocation>();

        public virtual EmploymentStatus? EmploymentStatus { get; set; }

        public virtual EmploymentType? EmploymentType { get; set; }


        public virtual ICollection<EmploymentContact> EmploymentContacts { get; set; } = new List<EmploymentContact>();
        protected Employment()
        {

        }
        public Employment(
              string _EmployeeCode,
         Guid _PersonId,
         Guid? _EmploymentTypeId,
         Guid? _EmploymentStatusId,
         DateOnly? _EffectiveFrom = null,
         DateOnly? _EffectiveTo = null
            )
        {
            EmployeeCode = _EmployeeCode;
            FkNaturalPersonId = _PersonId;
            FkEmploymentTypeId = _EmploymentTypeId;
            FkEmploymentStatusId = _EmploymentStatusId;

            EffectiveFrom = _EffectiveFrom ?? DateOnly.FromDateTime( DateTime.UtcNow);

            EffectiveTo = _EffectiveTo;

        }

        public bool ApplyChange(
            string? _employeeCode = null,
            Guid? _employmentTypeId = null,
            Guid? _employmentStatusId = null,
            DateOnly? _startDate = null,
            DateOnly? _endDate = null)
        {
         
            bool hasChange = false;

            if (_employeeCode != null && _employeeCode?.Trim() != EmployeeCode.Trim())
            {
                EmployeeCode = _employeeCode;
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
                this.EffectiveFrom =(DateOnly) _startDate;
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

