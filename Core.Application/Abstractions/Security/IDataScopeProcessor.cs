using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Abstractions.Security
{
    public interface IDataScopeProcessor
    {
        Task<IQueryable<TEntity>> ApplyScope<TEntity>(IQueryable<TEntity> query) where TEntity : class;
    }
}
