using Core.Domain.Common;
using Core.Domain.Enums;
using Core.Domain.Interfaces;
using Core.Shared.Enums.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Authorization
{
    public interface IAuthorizationProcessor<TEntity>
        where TEntity : class
    {

        Task<IQueryable<TEntity>> ApplyScope(IQueryable<TEntity> query) ;

        Task CheckPermissionAsync(TEntity entity, PermissionAction action);
        Task SetOwnerDefaults(TEntity entity) ;

    }
}
