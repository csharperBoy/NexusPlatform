using Authorization.Application.DTOs.Permissions;
using Core.Shared.Results;

namespace Authorization.Application.Interfaces
{
    /*
    📌 IAuthorizationService
    ------------------------
    سرویس تجمیعی سطح بالا که برای کنترل دسترسی API استفاده می‌شود.

    این سرویس:
    - PermissionEvaluator و DataScopeEvaluator را ترکیب می‌کند
    - نتیجه نهایی را در قالب UserAccessDto برمی‌گرداند
    - امکان استفاده در Attribute ها یا Middleware فراهم می‌شود

    🛠 متدها:
    1. GetUserEffectiveAccessAsync
       - دسترسی نهایی (Permission + DataScope) کاربر برای کل سیستم

    2. HasAccessAsync
       - بررسی دسترسی کاربر برای یک ResourceKey
   */

    public interface IAuthorizationService
    {
        Task<Result<UserAccessDto>> GetUserEffectiveAccessAsync(
            Guid userId,
            CancellationToken ct = default);

        Task<Result<bool>> HasAccessAsync(
            Guid userId,
            string resourceKey,
            CancellationToken ct = default);
    }
}
