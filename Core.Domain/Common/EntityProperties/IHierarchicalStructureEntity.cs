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
        TParentId ParentId { get;  }
        TEntity? Parent { get; }
        ICollection<TEntity>? Children { get;    }
        void ChangeParent(TParentId? newParentId);
    }
    public static class HierarchicalStructureEntityExtention
    {
        
    }
     
    }

//#region IHierarchicalStructureEntity Impelement
//public Guid? ParentId { get; private set; }
//public virtual Location? Parent { get; private set; }
//public virtual ICollection<Location> Children { get; private set; } = new List<Location>();
//public void ChangeParent(Guid? newParentId)
//{
//    if (newParentId == Id)
//        throw new InvalidOperationException("Menu cannot be its own parent.");

//    ParentId = newParentId;
//    Touch();

//    // ارسال ایونت وقتی ساختار سلسله مراتب تغییر می‌کند
//    //AddDomainEvent(new MenuHierarchyChangedEvent(Id));
//}
//#endregion
