using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Enums.Authorization
{
    public enum AssigneeType : byte
    {
        Person = 1,      // اولویت ۱: تنظیم خاص برای شخص
        Position = 2,    // اولویت ۲: پست سازمانی
        Role = 3 ,        // اولویت ۳: نقش کاربری
        User = 4
    }

}
