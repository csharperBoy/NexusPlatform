using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Domain.Entities
{
    /*
     اضافه شدن توضیحات و ترتیب نقش‌ها (برای پنل مدیریت نقش‌ها).

    قابل گسترش برای سیستم سلسله‌مراتب نقش‌ها در آینده.
     */
    public class ApplicationRole : IdentityRole<Guid>, IAggregateRoot , IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر
        #endregion
        public string? Description { get; private set; }
        public int? OrderNum { get; private set; }

        
        protected ApplicationRole() : base() { }

        public ApplicationRole(string name, string? description = null, int? orderNum = null)
            : base(name)
        {
            Description = description;
            OrderNum = orderNum;
            NormalizedName = name.ToUpperInvariant();
            
        }
        private void Touch() => ModifiedAt = DateTime.UtcNow;

        public void Update(string? description, int? orderNum)
        {
            Description = description;
            OrderNum = orderNum;
            Touch();
        }

        public bool ApplyChange(
             string _Name,
             string? _Description,
             int? _OrderNum
            )
        {
            bool hasChange = false;
            // آپدیت فیلدها
            if (_Name != null && _Name != this.Name)
            {
                this.Name = _Name;
                hasChange = true;
            }
            if (_Description != null && _Description != this.Description)
            {
                this.Description = _Description;
                hasChange = true;
            }

           
            if (_OrderNum != null && _OrderNum != this.OrderNum)
            {
                this.OrderNum = _OrderNum;
                hasChange = true;
            }
            
            if (hasChange)
            {
                Touch();
            }
            return hasChange;
        }
    }
}