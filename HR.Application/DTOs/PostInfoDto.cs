using Core.Shared.DTOs.Contact;
using Core.Shared.Enums.HR;
using Core.Shared.Enums.People;
using HR.Domain.Entities;
using HR.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Application.DTOs
{
    public class PostInfoDto
    {
        public Guid Id { get; set; }

        public string PostCode { get; set; } = null!;

        public Guid? ParentId { get; set; }

        public Guid? FkParentId { get; set; }

        public Guid FkJobTitleId { get; set; }

        public string? JobTitleName { get; set; }
        public Guid? FkOrganizationUnitId { get; set; }

        public string OrganizationUnitsName { get; set; } = null!;
        public Guid? FkJobLevelId { get; set; }

        public string? JobLevelTitle { get; set; }

        public Guid? FkGradeId { get; set; }

        public string? GradeTitle { get; set; }

        public Guid? FkCostCenterId { get; set; }

        public string? CostCenterName { get; set; }

        public Guid? EmploymentId { get; set; }
        public string? EmploymentCode { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int? Gender { get; set; }

        public PostAssignmentType? AssigneeType { get; set; }

        public List<EntityContactDto<HrContactType>> Contacts { get; set; } = null;
        public List<LocationInfoDto> locations { get;set; } = null;
    }
}
