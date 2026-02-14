using Authorization.Application.DTOs.DataScopes;
using Core.Domain.Enums;
using Core.Shared.Enums;
using Core.Shared.Enums.Authorization;
using Core.Shared.Results;

namespace Authorization.Application.Interfaces
{
    public interface IDataScopeEvaluator
    {
        /// <summary>
        /// پارامتر action اضافه شد
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="resourceKey"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        Task<ScopeType> EvaluateScopeAsync(string resourceKey, PermissionAction action);

        /// <summary>
        /// ارزیابی محدوده داده کاربر برای یک منبع
        /// </summary>
        Task<DataScopeDto> EvaluateDataScopeAsync( string resourceKey);

        /// <summary>
        /// ارزیابی تمام محدوده‌های داده کاربر
        /// </summary>
        Task<IReadOnlyList<DataScopeDto>> EvaluateAllDataScopesAsync();

        /// <summary>
        /// ساخت شرط WHERE برای فیلتر داده‌ها بر اساس محدوده کاربر
        /// </summary>
        Task<string> BuildDataFilterAsync( string resourceKey);
    }
}
