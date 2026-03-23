using Core.Shared.DTOs.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Authorization.PublicService
{
    public interface IPermissionPublicService
    {
       
        /// <summary>
        /// ثبت پرمیشن‌های اولیه برای یک نقش خاص (مثلاً ادمین)
        /// </summary>
        Task SeedRolePermissionsAsync( List<PermissionDto> permissions, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PermissionDto>> GetUserAllPermissionsAsync(Guid userId, Guid? personId, List<Guid>? positionsId, List<Guid> roleIds);
    }
}
