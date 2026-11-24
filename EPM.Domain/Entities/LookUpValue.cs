using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;

namespace EPM.Domain.Entities;

public partial class LookUpValue : BaseEntity, IAuditableEntity
{
    #region IAuditableEntity Impelement
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
    public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
    public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
    public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

    public void Touch() => ModifiedAt = DateTime.UtcNow;
    #endregion
    public int FkLookUpId { get; set; }

    public string Value { get; set; } = null!;

    public string Display { get; set; } = null!;

    public int OrderNum { get; set; }

    public string? Description { get; set; }

    public string? SystemInfo { get; set; }

    public bool FlgLogicalDelete { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<ClaimUserOnIndicator> ClaimUserOnIndicators { get; set; } = new List<ClaimUserOnIndicator>();

    public virtual ICollection<ClaimUserOnSystem> ClaimUserOnSystems { get; set; } = new List<ClaimUserOnSystem>();

    public virtual LookUp FkLookUp { get; set; } = null!;

    public virtual ICollection<Indicator> IndicatorFkLkpDesirabilities { get; set; } = new List<Indicator>();

    public virtual ICollection<Indicator> IndicatorFkLkpForms { get; set; } = new List<Indicator>();

    public virtual ICollection<Indicator> IndicatorFkLkpManualities { get; set; } = new List<Indicator>();

    public virtual ICollection<Indicator> IndicatorFkLkpMeasures { get; set; } = new List<Indicator>();

    public virtual ICollection<Indicator> IndicatorFkLkpPeriods { get; set; } = new List<Indicator>();

    public virtual ICollection<Indicator> IndicatorFkLkpTypes { get; set; } = new List<Indicator>();

    public virtual ICollection<IndicatorValue> IndicatorValueFkLkpShifts { get; set; } = new List<IndicatorValue>();

    public virtual ICollection<IndicatorValue> IndicatorValueFkLkpValueTypes { get; set; } = new List<IndicatorValue>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
