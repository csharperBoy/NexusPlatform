using Authorization.Domain.Enums;
using Core.Domain.Enums;
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
        public Guid Id { get; init; }
        public Guid ResourceId { get; init; }
        public AssigneeType AssigneeType { get; init; }
        public Guid AssigneeId { get; init; }
        public PermissionAction Action { get; init; }
        public bool IsAllow { get; init; }
        public bool IsActive { get; init; }
        public int Priority { get; init; }
        public string Condition { get; init; } = string.Empty;
        public DateTime? EffectiveFrom { get; init; }
        public DateTime? ExpiresAt { get; init; }
        public string Description { get; init; } = string.Empty;
        public int Order { get; init; }
        public DateTime CreatedAt { get; init; }
        public string CreatedBy { get; init; } = string.Empty;
    }
}
