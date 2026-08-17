using System;
using System.Collections.Generic;

namespace HR.Domain.Entities;

public partial class PostInfoView
{
    public Guid Id { get; set; }

    public string PostCode { get; set; } = null!;


    public Guid? FkParentId { get; set; }

    public Guid FkJobTitleId { get; set; }

    public Guid? FkOrganizationUnitId { get; set; }

    public Guid? FkJobLevelId { get; set; }

    public Guid? FkGradeId { get; set; }

    public Guid? FkCostCenterId { get; set; }

    public string? CostCenterName { get; set; }

    public string? GradeTitle { get; set; }

    public string? JobLevelTitle { get; set; }

    public string? JobTitleName { get; set; }


    public Guid? EmploymentId { get; set; }
    public string? EmploymentCode { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? NationalCode { get; set; }

    public int? Gender { get; set; }

    public int? AssignmentsAssigneeType { get; set; }

    public string OrganizationUnitsName { get; set; } = null!;
    
}
