using Core.Shared.Enums.Authorization;
using Core.Shared.Enums.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.Enums
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
        public static string GetIconString(this Icon icon)
        {
            //if (icon == null)
            //    return null;
            var field = typeof(Icon).GetField(icon.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? icon.ToString();
        }
        public static string GetIconString(this Icon? icon)
        {
            if (icon == null)
                return null;
            var field = typeof(Icon).GetField(icon.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? icon.ToString();
        }
    }
}
