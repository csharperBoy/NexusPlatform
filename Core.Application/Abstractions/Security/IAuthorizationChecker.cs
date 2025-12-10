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
    }
}
