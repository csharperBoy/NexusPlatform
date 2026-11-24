using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;

namespace EPM.Domain.Entities;

public partial class IndicatorValue : BaseEntity, IAuditableEntity
{
    #region IAuditableEntity Impelement
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
    public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
    public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
    public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

    public void Touch() => ModifiedAt = DateTime.UtcNow;
    #endregion
    public Guid FkIndicatorId { get; set; }

    public int FkLkpValueTypeId { get; set; }

    public int FkLkpShiftId { get; set; }

    public DateTime DateTime { get; set; }

    public decimal Value { get; set; }

    public decimal ValueCumulative { get; set; }

    public string? Description { get; set; }

    public string? SystemInfo { get; set; }

    public bool FlgLogicalDelete { get; set; }

    public virtual Indicator FkIndicator { get; set; } = null!;

    public virtual LookUpValue FkLkpShift { get; set; } = null!;

    public virtual LookUpValue FkLkpValueType { get; set; } = null!;
}
