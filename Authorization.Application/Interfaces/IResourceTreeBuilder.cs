using Authorization.Application.DTOs.Resource;
using Core.Shared.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Application.Interfaces
{ 
    /*
     📌 IResourceTreeBuilder
     -----------------------
     مسئول ساخت ساختار درختی Resource ها بر اساس ParentId های آن‌ها.

     🔍 کاربرد:
     - برای نمایش منوهای UI
     - برای بررسی ارث‌بری Permission
     - برای تولید Tree واکنش‌گرا در پنل مدیریت

     🛠 متد:
     BuildTreeAsync
        - تمام Resource ها را خوانده و مدل ResourceTreeDto تولید می‌کند.
    */

    public interface IResourceTreeBuilder
    {
        Task<Result<IReadOnlyList<ResourceTreeDto>>> BuildTreeAsync(CancellationToken ct = default);
    }
}
