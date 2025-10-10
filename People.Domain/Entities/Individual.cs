using System;
using System.Collections.Generic;

namespace People.Domain.Entities;

public partial class Individual
{
    public Guid Id { get; set; }

    public string? UserId { get; set; }

    public string? Name { get; set; }

    public string? Family { get; set; }

    public string? FatherName { get; set; }

    public short FklkpGenderId { get; set; }

    public string? NationalCode { get; set; }

    public string? OneTimePassword { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? LastModifiedBy { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public virtual ICollection<IndividualDetail> IndividualDetails { get; set; } = new List<IndividualDetail>();
}
