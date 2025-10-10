using System;
using System.Collections.Generic;

namespace People.Domain.Entities;

public partial class IndividualDetail
{
    public long Id { get; set; }

    public Guid IndividualId { get; set; }

    public long? PreviousVersionId { get; set; }

    public long? NextVersionId { get; set; }

    public bool IsCurrentVersion { get; set; }

    public DateTime VersionStartDate { get; set; }

    public DateTime? VersionEndDate { get; set; }

    public string? Mobile { get; set; }

    public string? Phone { get; set; }

    public string? EmergencyMobile { get; set; }

    public string? Mail { get; set; }

    public string? Address { get; set; }

    public short? PersonalCode { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public DateOnly? DateOfHire { get; set; }

    public DateOnly? DateOfMarried { get; set; }

    public short MemberQty { get; set; }

    public short DependantsQty { get; set; }

    public string? Descriptions { get; set; }

    public byte[]? Image { get; set; }

    public bool? Enablity { get; set; }

    public bool? Visiblity { get; set; }

    public bool? Remove { get; set; }

    public string? PasswordEmergency { get; set; }

    public int? StudentChildCount { get; set; }

    public bool? UseSpecial { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public virtual Individual Individual { get; set; } = null!;

    public virtual ICollection<IndividualDetail> InverseNextVersion { get; set; } = new List<IndividualDetail>();

    public virtual ICollection<IndividualDetail> InversePreviousVersion { get; set; } = new List<IndividualDetail>();

    public virtual IndividualDetail? NextVersion { get; set; }

    public virtual IndividualDetail? PreviousVersion { get; set; }
}
