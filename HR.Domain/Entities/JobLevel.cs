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
    public class JobLevel : BaseEntity
    {
        public string Code { get; private set; }
        public string Title { get; private set; }
        public int Order { get; private set; }
        public bool IsActive { get; private set; }

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        protected JobLevel() { }
    }
}
