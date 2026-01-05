using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Enums
{
    public static class EnumExtensions
    {
        public static ResourceType ToResourceType(this string typeStr)
        {
            return Enum.TryParse<ResourceType>(typeStr, true, out var val)
                ? val
                : ResourceType.Ui; // مقدار پیش‌فرض
        }

        public static ResourceCategory ToResourceCategory(this string catStr)
        {
            return Enum.TryParse<ResourceCategory>(catStr, true, out var val)
                ? val
                : ResourceCategory.System; // مقدار پیش‌فرض
        }
        public static TEnum ToEnumOrDefault<TEnum>(this string value, TEnum defaultValue) where TEnum : struct
        {
            return Enum.TryParse<TEnum>(value, true, out var result) ? result : defaultValue;
        }

    }

}
