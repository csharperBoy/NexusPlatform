using Core.Domain.Common.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR.Domain.Entities
{
    
    public class CostCenter :BaseEntity
    {
        public string Code { get; private set; }
        public string Name { get; private set; }
        public bool IsActive { get; private set; }
        protected CostCenter()
        {
            
        }
    }
}
