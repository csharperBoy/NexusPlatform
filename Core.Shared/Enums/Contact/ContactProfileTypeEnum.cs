using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Enums.Contact
{
    public enum ContactProfileTypeEnum : byte
    {
        Party = 0,      // اولویت ۱: تنظیم خاص برای شخص
        Post = 1,    // اولویت ۲: پست سازمانی
        Employment = 2,        // اولویت ۳: نقش کاربری
        Location = 3
    }
}
