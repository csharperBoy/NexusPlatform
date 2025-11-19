using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Enums
{
    public enum ResourceType
    {
        Module = 1, // ماژول
        Page = 2, // صفحه
        Component = 3, // کامپوننت/دکمه/جدول
        Action = 4 // عمل خاص
    }
}
