using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace PhoneBook.Domain.Entities;

[Keyless]
public partial class EmployementInfoView
{
    public Guid Id { get; set; }
    public string NationalCode { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string EmployeeCode { get; set; } = null!;

    public DateOnly EmployeeEffectiveFrom { get; set; }

    public DateOnly? EmployeeEffectiveTo { get; set; }

    public Guid PartyId { get; set; }

    public string? PartyMobile { get; set; }

    public string? PartyAddress { get; set; }

    public string? PartyPhone { get; set; }

    public string? PartyEmail { get; set; }

    public string? EmployeeStatusName { get; set; }

    public string? EmployeeTypeName { get; set; }

    public int AssignmentsAssigneeType { get; set; }

    public DateOnly AssignmentsEffectiveFrom { get; set; }

    public DateOnly? AssignmentsEffectiveTo { get; set; }

    public string PostCode { get; set; } = null!;

    public string? GradeTitle { get; set; }

    public string? CostCenterName { get; set; }

    public string? JobLevelTitle { get; set; }

    public string? JobTitleName { get; set; }

    public string? OrganizationUnitsName { get; set; }

    public string? PostContactPhone { get; set; }

    public string? PostContactMobile { get; set; }

    public string? PostContactEmail { get; set; }

    public string? PostContactFax { get; set; }

    public DateOnly? EmploymentLocationsEffectiveFrom { get; set; }

    public DateOnly? EmploymentLocationsEffectiveTo { get; set; }

    public string? LocationTitle { get; set; }
}
