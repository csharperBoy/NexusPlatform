using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    /// <summary>
    /// عنوان شغلی
    /// 
    /// مهندس نرم‌افزار، مدیر بازاریابی، حسابدار ارشد، مشاور حقوقی، اپراتور تولید
    /// </summary>
    public class JobTitle :BaseEntity 
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        protected JobTitle()
        {
            
        }
    }
}
