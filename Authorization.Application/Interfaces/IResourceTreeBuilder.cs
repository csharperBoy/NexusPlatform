using Authorization.Application.DTOs.Resource;
using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{
    public interface IResourceTreeBuilder
    {
        /// <summary>
        /// ساخت درخت کامل منابع
        /// </summary>
        Task<IReadOnlyList<ResourceTreeDto>> BuildTreeAsync();

        /// <summary>
        /// ساخت درخت منابع برای کاربر خاص (فقط منابع قابل دسترسی)
        /// </summary>
        Task<IReadOnlyList<ResourceTreeDto>> BuildTreeForUserAsync(Guid userId);

        /// <summary>
        /// پیدا کردن مسیر سلسله مراتبی یک منبع
        /// </summary>
        Task<IReadOnlyList<ResourceDto>> GetResourcePathAsync(string resourceKey);
    }
}
