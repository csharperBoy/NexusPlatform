using Authorization.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.DataScopes
{
    /*
   📌 DataScopeDto
   ----------------
   مدل انتقالی محدوده‌ی داده برای یک کاربر یا نقش.

   این DTO خلاصه‌ای از Entity اصلی DataScope است و برای:
   - Evaluatorها
   - API سطح نمایش
   - UserAccessDto
   استفاده می‌شود.
  */

    public class DataScopeDto
    {
        public Guid ResourceId { get; init; }
        public string ResourceKey { get; init; } = string.Empty;

        public ScopeType Scope { get; init; }

        // فقط برای ScopeType.SpecificUnit
        public Guid? SpecificUnitId { get; init; }

        // منبع اختصاص این DataScope (Role / Position / Person)
        public AssigneeType AssigneeType { get; init; }
        public Guid AssigneeId { get; init; }
    }
}
