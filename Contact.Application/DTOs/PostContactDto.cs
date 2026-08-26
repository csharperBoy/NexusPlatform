using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.HR;
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Application.DTOs
{
    public class PostContactDto
    {
        public Guid Id { get; set; }

        public Guid? FkParentId { get; set; }

        public string? CostCenterName { get; set; }

        public string? GradeTitle { get; set; }

        public string? JobLevelTitle { get; set; }

        public string? JobTitleName { get; set; }

        public List<string>? OfficePhone { get; set; }

        public List<string>? OrgMobile { get; set; }

        public List<string>? OrgEmail { get; set; }

        //public List<EntityContactDto<HrContactType>> Contacts { get; set; } = null;
        public string? EmploymentCode { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int? Gender { get; set; }

        public PostAssignmentType? AssignmentsAssigneeType { get; set; }

        public string OrganizationUnitsName { get; set; } = null!;
    }
}
