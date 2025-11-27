using Authorization.Application.DTOs.Permissions;
using Core.Shared.Results;

namespace Authorization.Application.Interfaces
{
    public interface IPermissionEvaluator
    {
        /// <summary>
        /// ارزیابی دسترسی کاربر برای یک منبع خاص
        /// </summary>
        Task<EffectivePermissionDto> EvaluateUserPermissionsAsync(Guid userId, string resourceKey);

        /// <summary>
        /// ارزیابی تمام دسترسی‌های کاربر
        /// </summary>
        Task<IReadOnlyList<EffectivePermissionDto>> EvaluateAllUserPermissionsAsync(Guid userId);

        /// <summary>
        /// بررسی سریع دسترسی برای یک action خاص
        /// </summary>
        Task<bool> HasPermissionAsync(Guid userId, string resourceKey, string action);
    }
}
