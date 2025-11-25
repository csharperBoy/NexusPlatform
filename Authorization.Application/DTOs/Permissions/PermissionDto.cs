using Authorization.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.DTOs.Permissions
{/*
     📌 PermissionDto
     -----------------
     نمایش ساده‌شده Permission جهت عملیات Evaluator.

     Domain Permission شامل فیلدهای زیاد است، اما برای Evaluator فقط
     این موارد نیاز است.
    */

    public class PermissionDto
    {
        public Guid ResourceId { get; init; }
        public string ResourceKey { get; init; } = string.Empty;

        public PermissionAction Action { get; init; }
        public bool IsAllow { get; init; }

        public AssigneeType AssigneeType { get; init; }
        public Guid AssigneeId { get; init; }

        public DateTime? ExpiresAt { get; init; }
        public DateTime? EffectiveFrom { get; init; }

        public bool IsActive { get; init; }
    }
}
