using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Enums
{
    public enum ScopeType : byte
    {
        // سطح ۱: هیچ داده‌ای (برای مواقعی که فقط دکمه را می‌بیند اما داده‌ای لود نمی‌شود)
        None = 0,

        Account = 1,
        // سطح ۲: فقط داده‌های خودش
        Self = 2,
        // سطح ۳: داده‌های واحد سازمانی خودش
        Unit = 3,

        // سطح ۴: واحد سازمانی خودش + تمام زیرمجموعه‌ها (سلسله‌مراتب)
        UnitAndBelow = 4,

        // سطح ۵: یک فیلد خاص از جدول (که در فیلد SpecificId ذخیره می‌شود)
        SpecificProperty = 5,

        // سطح ۶: همه داده‌ها (گلوبال)
        All = 99
    }
}
