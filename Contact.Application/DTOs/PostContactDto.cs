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

        public string? JobLevelTitle { get; set; }

        public string? JobTitleName { get; set; }

        public string? OfficePhone { get; set; }

        public string? OrgMobile { get; set; }

        public string? OrgEmail { get; set; }

        public string? EmploymentCode { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int? Gender { get; set; }

        public int? AssignmentsAssigneeType { get; set; }

        public string OrganizationUnitsName { get; set; } = null!;
    }
}
