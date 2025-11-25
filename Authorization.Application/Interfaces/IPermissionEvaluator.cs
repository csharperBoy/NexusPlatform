using Authorization.Application.DTOs.Permissions;
using Core.Shared.Results;

namespace Authorization.Application.Interfaces
{
    /*
     📌 IPermissionEvaluator
     -----------------------
     اینترفیس مسئول *محاسبه دسترسی مؤثر* (Effective Permissions) کاربر برای یک Resource یا مجموعه‌ای از Resource ها است.

     🔍 نقش این سرویس:
     - محاسبه ترکیبی دسترسی کاربر از منابع مختلف:
       ✔️ نقش‌ها (Role Permissions)
       ✔️ دسترسی‌های Explicit کاربر
       ✔️ دسترسی‌های ارث‌بری‌شده از والد Resourceها (Inheritance)
     - اعمال Merge Logic از طریق IPermissionMergeHelper
     - خروجی همیشه در قالب ساختار امن (DTO) است.

     🛠 متدها:
     1. EvaluateUserPermissionsAsync
        - بر اساس UserId و ResourceKey دسترسی واحد را محاسبه می‌کند.

     2. EvaluateAllUserPermissionsAsync
        - تمام دسترسی‌های مؤثر کاربر برای همه Resource ها را برمی‌گرداند.
        - مناسب برای UI/Frontend که نیاز دارد منو و صفحات را رندر کند.

     📌 نکته مهم:
     - این سرویس فقط *محاسبه* انجام می‌دهد و مسئول عملیات نوشتن نیست.
    */

    public interface IPermissionEvaluator
    {
        Task<Result<EffectivePermissionDto>> EvaluateUserPermissionsAsync(
            Guid userId,
            string resourceKey,
            CancellationToken ct = default);

        Task<Result<IReadOnlyList<EffectivePermissionDto>>> EvaluateAllUserPermissionsAsync(
            Guid userId,
            CancellationToken ct = default);
    }
}
