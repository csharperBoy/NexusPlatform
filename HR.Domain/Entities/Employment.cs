using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{

    public class Employment : BaseEntity
    {
        public string EmployeeCode { get; private set; }
        public Guid FkNaturalPersonId { get; private set; }
        public Guid FkEmploymentTypeId { get; private set; }
        public Guid FkEmploymentStatusId { get; private set; }
        public DateOnly EffectiveFrom { get; private set; }
        public DateOnly? EffectiveTo { get; private set; }

        //navigate
        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public virtual ICollection<EmploymentLocation> EmploymentLocations { get; set; } = new List<EmploymentLocation>();

        public virtual EmploymentStatus? EmploymentStatus { get; set; }

        public virtual EmploymentType? EmploymentType { get; set; }


        protected Employment()
        {

        }
        public Employment(
              string _EmployeeCode,
         Guid _PersonId,
         Guid _EmploymentTypeId,
         Guid _EmploymentStatusId,
         DateOnly _StartDate,
         DateOnly? _EndDate
            )
        {
            EmployeeCode = _EmployeeCode;
            PersonId = _PersonId;
            EmploymentTypeId = _EmploymentTypeId;
            EmploymentStatusId = _EmploymentStatusId;
            StartDate = _StartDate;
            EndDate = _EndDate;

        }

    }
}
