using Authorization.Domain.Entities;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    /*
    📌 ResourceByKeySpec
    ---------------------
    برای گرفتن یک Resource بر اساس Key درختی
    */

    public class ResourceByKeySpec : BaseSpecification<Resource>
    {
        public ResourceByKeySpec(string key)
            : base(r => r.Key == key)
        {
            // بارگذاری Permissions و DataScopes
            AddInclude(r => r.Permissions);

            // بارگذاری Parent و Children
            AddInclude(r => r.Parent);
            AddInclude(r => r.Children);
        }
    }
}
