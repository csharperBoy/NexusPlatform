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
        public string ResourceKey { get; init; } = string.Empty;
        public bool CanView { get; init; }
        public bool CanCreate { get; init; }
        public bool CanEdit { get; init; }
        public bool CanDelete { get; init; }
        public DateTime EvaluatedAt { get; init; }
        public int PermissionCount { get; init; }
        public bool HasExplicitDeny { get; init; }
    }
}
