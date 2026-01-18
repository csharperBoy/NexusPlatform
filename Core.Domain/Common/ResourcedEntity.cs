using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Common
{
    public   interface IResourcedEntity
    {
        Guid? EquivalentResourceId { get; }
    }
    public abstract class ResourcedEntity : AuditableEntity, IResourcedEntity
    {
        public Guid? EquivalentResourceId { get; protected set; }
        

        public void SetResource(Guid? resourceId)
        {
            EquivalentResourceId = resourceId;
            
        }
        
    }
}
