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
        public Guid PersonId { get; private set; }
        public Guid EmploymentTypeId { get; private set; }
        public Guid EmploymentStatusId { get; private set; }
        public DateOnly StartDate { get; private set; }
        public DateOnly? EndDate { get; private set; }
        public virtual ICollection<EmploymentLocations> employementLocations { get; private set; }
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
