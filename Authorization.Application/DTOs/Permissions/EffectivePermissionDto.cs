using Authorization.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Permissions
{
    /*
  📌 EffectivePermissionDto
  --------------------------
  نتیجه نهایی دسترسی کاربر برای یک resource.

  این خروجی بعد از ادغام:
  - نقش‌ها
  - دسترسی‌های صریح کاربر
  - ارث‌بری
  - قوانین Deny > Allow
  تولید می‌شود.
 */

    public class EffectivePermissionDto
    {
        public Guid ResourceId { get; init; }
        public string ResourceKey { get; init; } = string.Empty;

        // لیست اکشن‌هایی که کاربر اجازه دارد
        public IReadOnlyList<PermissionAction> AllowedActions { get; init; } =
            Array.Empty<PermissionAction>();

        // اگر حتی یک Deny وجود داشته باشد، این Resource قفل می‌شود
        public bool IsDenied { get; init; }

        // برای UI
        public bool HasFullAccess =>
            AllowedActions.Contains(PermissionAction.Full);
    }
}
