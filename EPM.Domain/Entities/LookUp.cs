using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;

namespace EPM.Domain.Entities;

public partial class LookUp : BaseEntity, IAuditableEntity
{
    #region IAuditableEntity Impelement
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
    public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
    public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
    public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

    public void Touch() => ModifiedAt = DateTime.UtcNow;
    #endregion
    public string Code { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? SystemInfo { get; set; }

    public bool FlgLogicalDelete { get; set; }

    public virtual ICollection<LookUpDestination> LookUpDestinations { get; set; } = new List<LookUpDestination>();

    public virtual ICollection<LookUpValue> LookUpValues { get; set; } = new List<LookUpValue>();
}
