using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Authorization
{
    public interface IPermissionPublicService
    {
       
        /// <summary>
        /// ثبت پرمیشن‌های اولیه برای یک نقش خاص (مثلاً ادمین)
        /// </summary>
        Task SeedRolePermissionsAsync( List<PermissionDefinition> permissions, CancellationToken cancellationToken = default);
    }
}
