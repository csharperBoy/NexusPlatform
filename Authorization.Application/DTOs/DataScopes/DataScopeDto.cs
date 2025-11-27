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
        public Guid Id { get; init; }
        public Guid ResourceId { get; init; }
        public string ResourceKey { get; init; } = string.Empty;
        public AssigneeType AssigneeType { get; init; }
        public Guid AssigneeId { get; init; }
        public ScopeType Scope { get; init; }
        public Guid? SpecificUnitId { get; init; }
        public string CustomFilter { get; init; } = string.Empty;
        public int Depth { get; init; } = 1;
        public bool IsActive { get; init; }
        public DateTime? EffectiveFrom { get; init; }
        public DateTime? ExpiresAt { get; init; }
        public string Description { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
        public DateTime? EvaluatedAt { get; init; }
        public int? DataScopeCount { get; init; }
    }
}
