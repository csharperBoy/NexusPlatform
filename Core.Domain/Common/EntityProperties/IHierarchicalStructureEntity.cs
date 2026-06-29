using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Common.EntityProperties
{
    public interface IHierarchicalStructureEntity<TEntity, TParentId>
     where TEntity : class
    {
        TParentId FkParentId { get; }
        TEntity? Parent { get; }
        ICollection<TEntity>? Children { get; }
        void ChangeParent(TParentId? newParentId);
    }
}
