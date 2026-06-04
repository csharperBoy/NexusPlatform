using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Attributes
{
   /// <summary>
   /// بر روی موجودیت ها تنظیم میشود و تعیین میکند که آیا امکان تنظیم rule بر روی مجوز ها وجود دارد یا نه؟
   /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public class DynamicFilterableAttribute : Attribute
    {
        public bool UseNavigation { get; set; } = false;
    }
}
