using Authorization.Application.DTOs.DataScopes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Permissions
{
    /*
      📌 UserAccessDto
      ------------------
      نتیجه کامل دسترسی‌های کاربر برای کل سیستم:

      شامل:
      - Effective Permissions
      - Data Scopes
      - خلاصه‌سازی مناسب برای UI / Middleware / Attribute ها
     */

    public class UserAccessDto
    {
        public Guid UserId { get; init; }

        public IReadOnlyList<EffectivePermissionDto> Permissions { get; init; } =
            Array.Empty<EffectivePermissionDto>();

        public IReadOnlyList<DataScopeDto> DataScopes { get; init; } =
            Array.Empty<DataScopeDto>();
    }
}
