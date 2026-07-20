using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    public class PostInfoView
    {
        public Guid? FkJobTitleId { get;  set; } = null!;
        public Guid? FkOrganizationUnitId { get;  set; } = null!;
        public Guid? FkJobLevelId { get;  set; } = null!;
        public Guid? FkGradeId { get;  set; } = null!;
        public Guid? FkCostCenterId { get;  set; } = null!;
        public string NationalCode { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string EmployeeCode { get; set; } = null!;

        public int AssignmentsAssigneeType { get; set; }

        public string PostCode { get; set; } = null!;

        public string? GradeTitle { get; set; }

        public string? CostCenterName { get; set; }

        public string? JobLevelTitle { get; set; }

        public string? JobTitleName { get; set; }

        public string? OrganizationUnitsName { get; set; }

        public string? OfficePhone { get; set; }

        public string? OrgMobile { get; set; }

        public string? OrgEmail { get; set; }


    }
}
