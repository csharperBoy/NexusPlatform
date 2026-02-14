using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs.Identity
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
        public AssigneeType AssigneeType { get;  set; }
        public Guid AssigneeId { get;  set; } 
        public Guid ResourceId { get;  set; }
        public string ResourceKey { get;  set; }
        public PermissionAction Action { get; set; }

        public ScopeType Scope { get; set; }
        public Guid SpecificScopeId { get; set; }
        public PermissionType Type { get; set; } 
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? ExpiresAt { get;   set; }

        public string? Description { get;   set; }
        public bool IsActive { get; set; }


        
    }
}
