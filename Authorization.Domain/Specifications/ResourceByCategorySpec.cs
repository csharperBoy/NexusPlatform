using Authorization.Domain.Entities;
using Authorization.Domain.Enums;
using Core.Domain.Specifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Domain.Specifications
{
    public class ResourceByCategorySpec : BaseSpecification<Resource>
    {
        public ResourceByCategorySpec(ResourceCategory? category = null)
            : base(r => r.IsActive && (!category.HasValue || r.Category == category.Value))
        {
            AddInclude(r => r.Permissions);
            AddInclude(r => r.DataScopes);
            AddInclude(r => r.Parent);
            AddInclude(r => r.Children);
            ApplyOrderBy(r => r.DisplayOrder);
            ApplyThenOrderBy(r => r.Name);
        }
    }
}
