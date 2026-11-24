using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;

namespace EPM.Domain.Entities;

public partial class ClaimUserOnIndicator : BaseEntity, IAuditableEntity
{
    #region IAuditableEntity Impelement
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
    public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
    public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
    public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

    public void Touch() => ModifiedAt = DateTime.UtcNow;
    #endregion
    public int FkLkpClaimUserOnIndicatorId { get; set; }

    public int FkUserId { get; set; }

    public Guid? FkIndicatorId { get; set; }

    public string? Description { get; set; }

    public string? SystemInfo { get; set; }

    public bool FlgLogicalDelete { get; set; }

    public virtual Indicator? FkIndicator { get; set; }

    public virtual LookUpValue FkLkpClaimUserOnIndicator { get; set; } = null!;

    public virtual User FkUser { get; set; } = null!;
}
