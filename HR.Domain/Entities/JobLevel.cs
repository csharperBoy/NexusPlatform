using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    /// <summary>
    /// سطح شغلی
    /// 
    /// کارشناس (۱)، کارشناس ارشد (۲)، سرپرست (۳)، مدیر (۴)، مدیرکل (۵)، معاون (۶)
    /// </summary>
    public class JobLevel : BaseEntity , IAuditableEntity
    {
        #region IAuditableEntity Impelement
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 📌 زمان ایجاد
        public string? CreatedBy { get; set; }                      // 📌 کاربر ایجادکننده
        public DateTime? ModifiedAt { get; set; }                   // 📌 زمان آخرین تغییر
        public string? ModifiedBy { get; set; }                     // 📌 کاربر آخرین تغییر

        public void Touch() => ModifiedAt = DateTime.UtcNow;
        #endregion
        public string Code { get; private set; }
        public string Title { get; private set; }
        public int? Order { get; private set; }
        public bool IsActive { get; private set; }

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        protected JobLevel() { }
        public JobLevel(
            string _Code,
            string _Title
            )
        {
            Code =_Code;
            Title = _Title;
            IsActive = true;
        }
        public void SetTitle(string _Title) { Title = _Title; }

        public bool ApplyChange(Optional<string?> _title, Optional<string?> _code)
        {
            bool hasChange = false;

            if (_title.IsSet && _title.Value?.Trim() != Title.Trim())
            {
                Title = _title.Value?.Trim();
                hasChange = true;
            }
            if (_code.IsSet && _code.Value?.Trim() != Code.Trim())
            {
                Code = _code.Value?.Trim();
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
