using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;

namespace EPM.Domain.Entities;

public partial class Indicator : BaseEntity, IAuditableEntity
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

    public DateTime DateTimeFrom { get; set; }

    public DateTime? DateTimeTo { get; set; }

    public int FkLkpTypeId { get; set; }

    public int FkLkpManualityId { get; set; }

    public int FkLkpPeriodId { get; set; }

    public int FkLkpMeasureId { get; set; }

    public int FkLkpDesirabilityId { get; set; }

    public int FkLkpFormId { get; set; }

    public string? Formula { get; set; }

    public string? Description { get; set; }

    public string? SystemInfo { get; set; }

    public bool FlgLogicalDelete { get; set; }

    public virtual ICollection<ClaimUserOnIndicator> ClaimUserOnIndicators { get; set; } = new List<ClaimUserOnIndicator>();

    public virtual LookUpValue FkLkpDesirability { get; set; } = null!;

    public virtual LookUpValue FkLkpForm { get; set; } = null!;

    public virtual LookUpValue FkLkpManuality { get; set; } = null!;

    public virtual LookUpValue FkLkpMeasure { get; set; } = null!;

    public virtual LookUpValue FkLkpPeriod { get; set; } = null!;

    public virtual LookUpValue FkLkpType { get; set; } = null!;

    public virtual ICollection<IndicatorCategory> IndicatorCategories { get; set; } = new List<IndicatorCategory>();

    public virtual ICollection<IndicatorValue> IndicatorValues { get; set; } = new List<IndicatorValue>();

    public virtual ICollection<MapItem> MapItems { get; set; } = new List<MapItem>();
}
