using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Users
{
    /*
   📌 UserPermissionsSummaryDto
   ------------------------------
   خلاصه‌ای از سطح دسترسی کاربر.

   مورد استفاده:
   - صفحه User Management
   - گزارش‌ها
   - View خلاصه شده از Effective Access
  */

    public class UserPermissionsSummaryDto
    {
        public Guid UserId { get; init; }

        public IReadOnlyList<string> AllowedResources { get; init; } =
            Array.Empty<string>();

        public IReadOnlyList<string> DeniedResources { get; init; } =
            Array.Empty<string>();

        public bool HasFullSystemAccess { get; init; }
    }
}
