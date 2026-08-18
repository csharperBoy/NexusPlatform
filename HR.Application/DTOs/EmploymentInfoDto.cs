using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.DTOs
{
    public class EmploymentInfoDto
    {
        public Guid Id { get; set; }
        public string NationalCode { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string EmploymentCode { get; set; } = null!;

        public DateOnly EmploymentEffectiveFrom { get; set; }

        public DateOnly? EmploymentEffectiveTo { get; set; }

        public Guid PartyId { get; set; }

        public string? EmploymentStatusName { get; set; }

        public string? EmploymentTypeName { get; set; }

        public int AssignmentsAssigneeType { get; set; }

        public DateTime AssignmentsEffectiveFrom { get; set; }

        public DateTime? AssignmentsEffectiveTo { get; set; }

        public string PostCode { get; set; } = null!;

        public string? GradeTitle { get; set; }

        public string? CostCenterName { get; set; }

        public string? JobLevelTitle { get; set; }

        public string? JobTitleName { get; set; }

        public string? OrganizationUnitsName { get; set; }

        public List<EntityContactDto<HrContactType>> Contacts { get; set; } = null;
        public List<EntityContactDto<PartyContactType>> partyContacts { get; set; } = null;
        public List<LocationInfoDto> locations { get; set; } = null;
    }
}
