using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Security
{
    public interface IPermissionChecker
    {
        /// <summary>
        /// بررسی می‌کند که آیا کاربر فعلی دارای مجوز مشخص‌شده هست یا نه.
        /// </summary>
        Task<bool> HasPermissionAsync(string permission);

        /// <summary>
        /// بررسی می‌کند که آیا کاربر فعلی حداقل یکی از مجوزهای داده‌شده را دارد یا نه.
        /// </summary>
        Task<bool> HasAnyPermissionAsync(params string[] permissions);
    }
}