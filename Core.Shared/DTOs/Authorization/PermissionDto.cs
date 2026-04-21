using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared.DTOs.Authorization
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

        public PermissionEffect Effect { get;  set; }  // برای Deny یا allow کردن


        public DateTime? EffectiveFrom { get; set; }
        public DateTime? ExpiresAt { get;   set; }

        public string? Description { get;   set; }
        public bool IsActive { get; set; }



        public List<ScopeDto> Scopes { get; set; } = new();
        //public List<PermissionRuleDto> Rules { get; set; } = new();
        public bool AppliesTo(AssigneeType assigneeType, Guid assigneeId)
        {
            return AssigneeType == assigneeType && AssigneeId == assigneeId;
        }
        public bool AppliesTo(AssigneeType assigneeType, List<Guid> assigneeId)
        {
            return AssigneeType == assigneeType && assigneeId.Any(a => a == AssigneeId);
        }

    }
}
