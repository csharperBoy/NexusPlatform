using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;

namespace EPM.Domain.Entities;

public partial class MapItem : BaseEntity, IAuditableEntity
{
    #region IAuditableEntity Impelement
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
    public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
    public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
    public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

    public void Touch() => ModifiedAt = DateTime.UtcNow;
    #endregion
    public Guid? FkIndicatorId { get; set; }

    public Guid? FkCategoryId { get; set; }

    public Guid? FkIndicatorCategoryId { get; set; }

    public DateTime? DateTimeFrom { get; set; }

    public DateTime? DateTimeTo { get; set; }

    public decimal? Weight { get; set; }

    public decimal? LowBound { get; set; }

    public decimal? HighBound { get; set; }

    public string? Description { get; set; }

    public string? SystemInfo { get; set; }

    public bool FlgLogicalDelete { get; set; }

    public virtual Category? FkCategory { get; set; }

    public virtual Indicator? FkIndicator { get; set; }

    public virtual IndicatorCategory? FkIndicatorCategory { get; set; }
}
