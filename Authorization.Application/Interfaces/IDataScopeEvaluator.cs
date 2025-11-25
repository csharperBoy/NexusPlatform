using Authorization.Application.DTOs.DataScopes;
using Core.Shared.Results;

namespace Authorization.Application.Interfaces
{
    /*
    📌 IDataScopeEvaluator
    ----------------------
    سرویس محاسبه DataScope نهایی کاربر برای یک Resource.
    ترکیب مشابه Permission است ولی محدوده داده را کنترل می‌کند.

    🧠 ورودی‌های DataScope ممکن است از:
      - نقش‌ها
      - دسترسی‌های صریح
      - ارث‌بری Resource
      - محدودیت‌های سیستمی (System-level limits)

    🛠 متدها:
    1. EvaluateDataScopeAsync
       - DataScope نهایی برای یک Resource.

    2. EvaluateAllDataScopesAsync
       - خروجی کامل جهت API سطح بالا که تمام DataScopeهای یک کاربر را برگرداند.
   */

    public interface IDataScopeEvaluator
    {
        Task<Result<DataScopeDto>> EvaluateDataScopeAsync(
            Guid userId,
            string resourceKey,
            CancellationToken ct = default);

        Task<Result<IReadOnlyList<DataScopeDto>>> EvaluateAllDataScopesAsync(
            Guid userId,
            CancellationToken ct = default);
    }
}
