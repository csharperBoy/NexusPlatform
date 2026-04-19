using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Enums.Authorization
{
    public enum AssigneeType : byte
    {
        Person = 0,      // اولویت ۱: تنظیم خاص برای شخص
        Position = 1,    // اولویت ۲: پست سازمانی
        Role = 2 ,        // اولویت ۳: نقش کاربری
        User = 3
    }

}
