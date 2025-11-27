using Authorization.Application.DTOs.Permissions;
using Core.Shared.Results;

namespace Authorization.Application.Interfaces
{
    public interface IAuthorizationService
    {
        /// <summary>
        /// بررسی سریع دسترسی کاربر به یک منبع
        /// استفاده در: AuthorizeResourceAttribute
        /// </summary>
        Task<bool> CheckAccessAsync(Guid userId, string resourceKey, string action);

        /// <summary>
        /// بررسی پیشرفته دسترسی با context اضافی
        /// </summary>
        Task<AccessResult> CheckAccessAsync(AccessRequest request);

        /// <summary>
        /// دریافت تمام دسترسی‌های مؤثر کاربر
        /// استفاده در: پنل مدیریت و گزارش‌گیری
        /// </summary>
        Task<UserAccessDto> GetUserEffectiveAccessAsync(Guid userId);

        /// <summary>
        /// بررسی دسترسی به چندین منبع به صورت همزمان
        /// </summary>
        Task<bool> CheckMultipleAccessAsync(Guid userId, IEnumerable<(string Resource, string Action)> permissions);
    }

    public class AccessRequest
    {
        public Guid UserId { get; set; }
        public string ResourceKey { get; set; }
        public string Action { get; set; }
        public Dictionary<string, object> Context { get; set; } = new();
    }

    public class AccessResult
    {
        public bool HasAccess { get; set; }
        public string DenyReason { get; set; }
        public Dictionary<string, object> Details { get; set; } = new();

        public static AccessResult Grant() => new() { HasAccess = true };
        public static AccessResult Deny(string reason) => new() { HasAccess = false, DenyReason = reason };
    }
}
