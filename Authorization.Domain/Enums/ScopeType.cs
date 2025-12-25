using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Enums
{
    public enum ScopeType : byte
    {
        // سطح ۱: هیچ داده‌ای (برای مواقعی که فقط دکمه را می‌بیند اما داده‌ای لود نمی‌شود)
        None = 0,

        // سطح ۲: فقط داده‌های خودش
        Self = 1,

        // سطح ۳: داده‌های واحد سازمانی خودش
        Unit = 2,

        // سطح ۴: واحد سازمانی خودش + تمام زیرمجموعه‌ها (سلسله‌مراتب)
        UnitAndBelow = 3,

        // سطح ۵: یک واحد خاص (که در فیلد SpecificId ذخیره می‌شود)
        SpecificUnit = 4,

        // سطح ۶: همه داده‌ها (گلوبال)
        All = 99
    }
}
