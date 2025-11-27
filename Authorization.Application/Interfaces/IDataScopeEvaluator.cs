using Authorization.Application.DTOs.DataScopes;
using Core.Shared.Results;

namespace Authorization.Application.Interfaces
{
    public interface IDataScopeEvaluator
    {
        /// <summary>
        /// ارزیابی محدوده داده کاربر برای یک منبع
        /// </summary>
        Task<DataScopeDto> EvaluateDataScopeAsync(Guid userId, string resourceKey);

        /// <summary>
        /// ارزیابی تمام محدوده‌های داده کاربر
        /// </summary>
        Task<IReadOnlyList<DataScopeDto>> EvaluateAllDataScopesAsync(Guid userId);

        /// <summary>
        /// ساخت شرط WHERE برای فیلتر داده‌ها بر اساس محدوده کاربر
        /// </summary>
        Task<string> BuildDataFilterAsync(Guid userId, string resourceKey);
    }
}
