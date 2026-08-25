using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace HR.Domain.Entities;

[Keyless]
public partial class EmploymentInfoView
{
    public Guid Id { get; set; }

    public Guid? FkContactProfileId { get;  set; }
    public Guid? FkPartyContactProfileId { get;  set; }
    public string NationalCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string EmploymentCode { get; set; } = null!;

    public DateOnly EmploymentEffectiveFrom { get; set; }

    public DateOnly? EmploymentEffectiveTo { get; set; }

    public Guid PartyId { get; set; }

    //public string? PartyMobile { get; set; }

    //public string? PartyAddress { get; set; }

    //public string? PartyPhone { get; set; }

    //public string? PartyEmail { get; set; }

    public string? EmploymentStatusName { get; set; }

    public string? EmploymentTypeName { get; set; }

    public int? AssignmentsAssigneeType { get; set; }

    public DateTime? AssignmentsEffectiveFrom { get; set; }

    public DateTime? AssignmentsEffectiveTo { get; set; }

    public string? PostCode { get; set; } = null!;

    public string? GradeTitle { get; set; }

    public string? CostCenterName { get; set; }

    public string? JobLevelTitle { get; set; }

    public string? JobTitleName { get; set; }

    public string? OrganizationUnitsName { get; set; }

}
