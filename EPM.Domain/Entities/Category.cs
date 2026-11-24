using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;

namespace EPM.Domain.Entities;

public partial class Category : BaseEntity , IAuditableEntity
{
    #region IAuditableEntity Impelement
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
    public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
    public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
    public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

    public void Touch() => ModifiedAt = DateTime.UtcNow;
    #endregion
    public int FklkpTypeId { get; set; }

    public Guid? FkParentId { get; set; }

    public string? Code { get; set; }

    public string? Title { get; set; }

    public byte? OrderNum { get; set; }

    public string? Description { get; set; }

    public bool FlgLogicalDelete { get; set; }

    public virtual Category? FkParent { get; set; }

    public virtual LookUpValue FklkpType { get; set; } = null!;

    public virtual ICollection<IndicatorCategory> IndicatorCategories { get; set; } = new List<IndicatorCategory>();

    public virtual ICollection<Category> InverseFkParent { get; set; } = new List<Category>();

    public virtual ICollection<MapItem> MapItems { get; set; } = new List<MapItem>();
}
