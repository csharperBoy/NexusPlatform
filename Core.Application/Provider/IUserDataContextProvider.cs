using Core.Application.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Provider
{
    public interface IUserDataContextProvider
    {
        /// <summary>
        /// Snapshot کامل از UserDataContext برای یک userId.
        /// برای Command/Query handlers و BackgroundService.
        /// </summary>
        Task<UserDataContext> GetAsync(CancellationToken ct);

        /// <summary>پر کردن Scoped Instance از HttpContext (معمولاً توسط Pipeline Behavior).</summary>
        Task SetUserData(CancellationToken ct);

        /// <summary>پر کردن Scoped Instance برای یک userId مشخص — Background flow.</summary>
        Task SetUserData(Guid userId, CancellationToken ct);

        /// <summary>ساخت Snapshot کامل بدون دست‌کاری Scoped Instance.</summary>
        Task<UserDataContext> BuildAsync(Guid userId, CancellationToken ct);
    }

}
