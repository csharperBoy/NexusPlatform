using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Security
{
    /// <summary>
    /// رابط ساده برای بررسی دسترسی - برای استفاده در Attributeها و ماژول‌های دیگر
    /// </summary>
    public interface IAuthorizationChecker
    {
        Task<bool> CheckAccessAsync(Guid userId, string resourceKey, string action);
        Task<bool> CheckAccessAsync(string resourceKey, string action);

        /// <summary>
        /// متدی که اسکوپ کاربر برای یک اکشن خاص روی یک ریسورس را برمی‌گرداند
        /// </summary>
        Task<ScopeType> GetPermissionScopeAsync(Guid userId, string resourceKey, PermissionAction action);
        Task<List<ScopeType>> GetScopeForUser(Guid userId, string resourceKey);
    }
}
