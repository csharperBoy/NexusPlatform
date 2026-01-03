using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Enums
{
    public enum ResourceType : byte
    {
        Module = 0, // ماژول اصلی
        Ui = 1,    // صفحات، دکمه‌ها، گریدها
        Data = 2   // داده‌ها برای Scoping
    }
}
