using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Common
{
    public class DataScopedAndResourcedEntity : DataScopedEntity, IResourcedEntity
    {
        public Guid? EquivalentResourceId { get; protected set; }


        public void SetResource(Guid? resourceId)
        {
            EquivalentResourceId = resourceId;

        }
    }
}
